using CoreLib;

namespace Mini3DCad
{

    /// <summary>
    /// 押出プリミティブ
    /// </summary>
    public class ExtrusionPrimitive : Primitive
    {
        public Polygon3D mPolygon;
        public List<Polygon3D> mInnerPolygon;
        public Point3D mVector;
        public FACE3D mSrcFace;
        public bool mLoop = true;       //  Polyline(false)とPolygon(true)

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ExtrusionPrimitive()
        {
            mPolygon = new Polygon3D();
            mInnerPolygon = new List<Polygon3D>();
            mVector = new Point3D();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="polygon">ポリゴン</param>
        /// <param name="srcFace">参照データの作成面</param>
        /// <param name="v">押出ベクトル</param>
        /// <param name="color">色</param>
        /// <param name="loop">Polygon</param>
        /// <param name="face">作成面</param>
        public ExtrusionPrimitive(Polygon3D polygon, List<Polygon3D> innerPolygon, Point3D v, bool loop = true)
        {
            mPrimitiveId = PrimitiveId.Extrusion;
            if (polygon == null)
                mPolygon = new Polygon3D();
            else
                mPolygon = polygon.toCopy();
            mPolygon.squeeze();
            if (innerPolygon == null)
                mInnerPolygon = new List<Polygon3D>();
            else
                mInnerPolygon = innerPolygon;
            mVector = v;
            mLoop = loop;
        }

        /// <summary>
        /// 3D座標リストの作成
        /// </summary>
        public override void createSurfaceData()
        {
            mSurfaceDataList = new List<SurfaceData>();
            if (mWireFrame) {
                //  ワイヤフレーム表示
                if (0 < mInnerPolygon.Count) {
                    foreach (var polygon in mInnerPolygon)
                        createWireFrame(polygon);
                } else {
                    createWireFrame(mPolygon);
                }
            } else {
                //  サーフェース表示
                if (0 < mInnerPolygon.Count) {
                    createHollSurface(mInnerPolygon);
                    createSideSurface(mInnerPolygon);
                } else {
                    createSurface(mPolygon);
                }
            }
        }

        /// <summary>
        /// 多孔ポリゴン3D平面
        /// </summary>
        /// <param name="polygons">ポリゴンリスト</param>
        private void createHollSurface(List<Polygon3D> polygons)
        {
            SurfaceData surfaceData = new SurfaceData();
            Polygon3D polygon = polygons[0];
            List<Polygon3D> innerPolygon = new List<Polygon3D>();
            innerPolygon = polygons.Skip(1).ToList();
            //  上面
            surfaceData.mVertexList = polygon.holePlate2Quads(innerPolygon);
            bool reverse = surfaceWise(surfaceData.mVertexList, mVector);
            reverse = mReverse ? !reverse : reverse;
            surfaceData.mDrawType = DRAWTYPE.QUADS;
            surfaceData.mFaceColor = mFaceColors[0];
            surfaceData.reverse(reverse);
            mSurfaceDataList.Add(surfaceData);
            //  下面
            surfaceData = new SurfaceData();
            surfaceData.mVertexList = mSurfaceDataList[mSurfaceDataList.Count - 1].mVertexList.ConvertAll(p => p.toCopy());
            surfaceData.mVertexList.ForEach(p => p.translate(mVector));
            reverse = surfaceWise(surfaceData.mVertexList, mVector);
            reverse = mReverse ? !reverse : reverse;
            surfaceData.mDrawType = DRAWTYPE.QUADS;
            surfaceData.mFaceColor = mFaceColors[0];
            surfaceData.reverse(!reverse);
            mSurfaceDataList.Add(surfaceData);
        }

        /// <summary>
        /// 面の向きが押出方向と同じかを求める
        /// </summary>
        /// <param name="vertex">座標リスト</param>
        /// <param name="vec">押出方向</param>
        /// <returns>押出方向に対する面の向き</returns>
        private bool surfaceWise(List<Point3D> vertex, Point3D vec)
        {
            int n = 1;
            List<Point3D> plist = new List<Point3D>() { vertex[0] };
            if (plist[0].length(vertex[n]) > 1e-6)
                plist.Add(vertex[n]);
            else
                plist.Add(vertex[++n]);
            if (plist[1].length(vertex[++n]) > 1e-6)
                plist.Add(vertex[n]);
            else
                plist.Add(vertex[++n]);
            Point3D normal = vertex[0].getNormal(vertex[1], vertex[2]);
            return (Math.PI / 2) > normal.angle(vec);
        }

        /// <summary>
        /// 複数ポリゴンの3D側面表示
        /// </summary>
        /// <param name="polygons">ポリゴンリスト</param>
        private void createSideSurface(List<Polygon3D> polygons)
        {
            for (int i = 0; i < polygons.Count; i++) {
                SurfaceData surfaceData = new SurfaceData();
                surfaceData.mVertexList = polygons[i].sideFace2Quads(mVector);
                surfaceData.mDrawType = DRAWTYPE.QUADS;
                surfaceData.mFaceColor = mFaceColors[0];
                surfaceData.reverse(!mReverse);
                mSurfaceDataList.Add(surfaceData);
            }
        }

        /// <summary>
        /// 単ポリゴンの3Dサーフェース
        /// </summary>
        /// <param name="polygon">ポリゴン</param>
        private void createSurface(Polygon3D polygon)
        {
            SurfaceData surfaceData;
            Point3D normal = polygon.getNormalLine();
            bool wise = (Math.PI / 2) > normal.angle(mVector);
            if (mEdgeDisp && mLoop) {
                //  1面(端面)
                mSurfaceDataList.Add(createSurfaceData(mPolygon, mVector, mDivideAngle, mFaceColors[0]));
                //  2面(端面)
                Polygon3D polygon2 = polygon.toCopy();
                polygon2.mCp += mVector;
                Point3D vector = mVector.toCopy();
                vector.inverse();
                mSurfaceDataList.Add(createSurfaceData(polygon2, vector, mDivideAngle, mFaceColors[0]));
            }
            //  側面
            surfaceData = new SurfaceData();
            surfaceData.mVertexList = new List<Point3D>();
            List<Point3D> outline = polygon.toPoint3D(mDivideAngle);
            for (int i = 0; i < outline.Count; i++) {
                Point3D np = outline[i].toCopy();
                np.translate(mVector);
                if (!wise) {
                    surfaceData.mVertexList.Add(outline[i]);
                    surfaceData.mVertexList.Add(np);
                } else {
                    surfaceData.mVertexList.Add(np);
                    surfaceData.mVertexList.Add(outline[i]);
                }
            }
            if (mLoop) {
                //  Polygonとして始終点を接続
                Point3D np = outline[0].toCopy();
                np.translate(mVector);
                if (!wise) {
                    surfaceData.mVertexList.Add(outline[0]);
                    surfaceData.mVertexList.Add(np);
                } else {
                    surfaceData.mVertexList.Add(np);
                    surfaceData.mVertexList.Add(outline[0]);
                }
            }
            surfaceData.mDrawType = DRAWTYPE.QUAD_STRIP;
            surfaceData.mFaceColor = mFaceColors[0];
            surfaceData.reverse(mReverse);
            mSurfaceDataList.Add(surfaceData);
        }

        /// <summary>
        /// 3Dワイヤフレーム
        /// </summary>
        /// <param name="polygon">ポリゴン</param>
        private void createWireFrame(Polygon3D polygon)
        {
            SurfaceData surfaceData;
            Polyline3D polyline1 = new Polyline3D(polygon);
            surfaceData = new SurfaceData();
            surfaceData.mVertexList = polyline1.toPoint3D(mDivideAngle);
            if (!mLoop)
                surfaceData.mVertexList.RemoveAt(surfaceData.mVertexList.Count - 1);
            surfaceData.mDrawType = DRAWTYPE.LINE_STRIP;
            surfaceData.mFaceColor = mFaceColors[0];
            mSurfaceDataList.Add(surfaceData);
            Polyline3D polyline2 = polyline1.toCopy();
            polyline2.translate(mVector);
            surfaceData = new SurfaceData();
            surfaceData.mVertexList = polyline2.toPoint3D(mDivideAngle);
            if (!mLoop)
                surfaceData.mVertexList.RemoveAt(surfaceData.mVertexList.Count - 1);
            surfaceData.mDrawType = DRAWTYPE.LINE_STRIP;
            surfaceData.mFaceColor = mFaceColors[0];
            mSurfaceDataList.Add(surfaceData);
            //  側面
            surfaceData = new SurfaceData();
            List<Point3D> plist = new List<Point3D>();
            for (int i = 0; i < polygon.mPolygon.Count; i++) {
                Point3D p1 = polyline1.toPoint3D(i);
                Point3D p2 = p1.toCopy();
                p2.add(mVector);
                plist.Add(p1);
                plist.Add(p2);
            }
            surfaceData.mVertexList = plist;
            surfaceData.mDrawType = DRAWTYPE.LINES;
            surfaceData.mFaceColor = mFaceColors[0];
            mSurfaceDataList.Add(surfaceData);
        }

        /// <summary>
        /// 2D表示用座標リストの作成
        /// </summary>
        public override void createVertexData()
        {
            mVertexList = new List<List<Point3D>>();
            if (mOutlineDisp) {
                //  3D元データ表示
                mOutlineColors.Clear();
                mOutlineType.Clear();
                if (0 < mPolygon.mPolygon.Count) {
                    mVertexList.Add(mPolygon.toPoint3D(mDivideAngle, mLoop));
                    mOutlineColors.Add(mLineColor);
                    mOutlineType.Add(mLineType);
                }
                for (int i = 0; i < mInnerPolygon.Count; i++) {
                    mVertexList.Add(mInnerPolygon[i].toPoint3D(mDivideAngle, mLoop));
                    mOutlineColors.Add(mLineColor);
                    mOutlineType.Add(mLineType);
                }
                mVertexList.Add(getVector().toPoint3D());
                mOutlineColors.Add(System.Windows.Media.Brushes.Green);
                mOutlineType.Add(1);
            } else {
                if (0 < mInnerPolygon.Count) {
                    for (int i = 0; i < mInnerPolygon.Count; i++) {
                        create2DSurfaceData(mInnerPolygon[i], mVector);
                        create2DSideData(mInnerPolygon[i], mVector);
                    }
                } else {
                    create2DSurfaceData(mPolygon, mVector);
                    create2DSideData(mPolygon, mVector);
                }
            }
        }

        /// <summary>
        /// ポリゴンの3D枠(端面)の登録
        /// </summary>
        /// <param name="polygon">ポリゴン</param>
        /// <param name="vec">押出ベクトル</param>
        private void create2DSurfaceData(Polygon3D polygon, Point3D vec)
        {
            if (mSurfaceVertex) {
                //  Debug(Surface表示確認)
                foreach (var surface in mSurfaceDataList)
                    mVertexList.AddRange(surface.toPolylineList());
            } else {
                //  1面(端面)
                Polygon3D polygon1 = polygon.toCopy();
                mVertexList.Add(polygon1.toPoint3D(mDivideAngle, mLoop));
                mVertexList.Add(polygon1.toPoint3D(mDivideAngle, mLoop));
                //  2面(端面)
                Polygon3D polygon2 = polygon.toCopy();
                polygon2.translate(vec);
                mVertexList.Add(polygon2.toPoint3D(mDivideAngle, mLoop));
                mVertexList.Add(polygon2.toPoint3D(mDivideAngle, mLoop));
            }
        }

        /// <summary>
        /// ポリゴンの3D枠(側面)の登録
        /// </summary>
        /// <param name="polygon">ポリゴン</param>
        /// <param name="vec">押出ベクトル</param>
        private void create2DSideData(Polygon3D polygon, Point3D vec)
        {
            //  側面
            Polygon3D polygon1 = polygon.toCopy();
            for (int i = 0; i < polygon1.mPolygon.Count; i++) {
                Point3D p1 = polygon1.toPoint3D(i);
                Point3D p2 = p1.toCopy();
                p2.add(vec);
                List<Point3D> plist = new List<Point3D>() { p1, p2 };
                mVertexList.Add(plist);
            }
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">2D平面</param>
        public override void translate(Point3D v, PointD pickPos, FACE3D face)
        {
            mPolygon.translate(v);
            for (int i = 0; i < mInnerPolygon.Count; i++)
                mInnerPolygon[i].translate(v);
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">2D平面</param>
        public override void rotate(Point3D cp, double ang, PointD pickPos, FACE3D face)
        {
            mVector.rotate(new Point3D(0, 0, 0), ang, face);
            mPolygon.rotate(cp, ang, face);
            for (int i = 0; i < mInnerPolygon.Count; i++)
                mInnerPolygon[i].rotate(cp, ang, face);
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">2D平面</param>
        public override void offset(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            mPolygon.offset(sp, ep);
            for (int i = 0; i < mInnerPolygon.Count; i++)
                mInnerPolygon[i].offset(sp, ep);
        }

        /// <summary>
        /// 座標を反転する
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">2D平面</param>
        public override void mirror(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            Line3D l = new Line3D(new Point3D(0, 0, 0), ep - sp);
            mVector = l.mirror(mVector,face);
            mPolygon.mirror(new Line3D(sp, ep), face);
            for (int i = 0; i < mInnerPolygon.Count; i++)
                mInnerPolygon[i].mirror(new Line3D(sp, ep), face);
        }

        /// <summary>
        /// トリム
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">2D平面</param>
        public override void trim(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {

        }

        /// <summary>
        /// 拡大縮小
        /// </summary>
        /// <param name="cp">拡大中心</param>
        /// <param name="scale">倍率</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">2D平面</param>
        public override void scale(Point3D cp, double scale, PointD pickPos, FACE3D face)
        {
            if (mOutlineDisp) {
                int select = pickSelect(pickPos, face);
                if (select == 0) {
                    mVector.length(mVector.length() * scale);
                } else {
                    mPolygon.scale(cp, scale);
                }
            } else {
                mVector.length(mVector.length() * scale);
                mPolygon.scale(cp, scale);
                for (int i = 0; i < mInnerPolygon.Count; i++)
                    mInnerPolygon[i].scale(cp, scale);
            }
        }

        /// <summary>
        /// ストレッチ
        /// </summary>
        /// <param name="vec">ストレッチベクトル</param>
        /// <param name="arc">円弧変形</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">2D平面</param>
        public override void stretch(Point3D vec, bool arc, PointD pickPos, FACE3D face)
        {
            if (mOutlineDisp) {
                int select = pickSelect(pickPos, face);
                if (select == 0) {
                    Line3D vec3D = getVector();
                    vec3D.stretch(vec, new Point3D(pickPos, face));
                    mVector = vec3D.mV.toCopy();
                } else {
                    if (select == 1)
                        mPolygon.stretch(vec, new Point3D(pickPos, face), arc);
                    else if (1 < select)
                        mInnerPolygon[select - 2].stretch(vec, new Point3D(pickPos, face), arc);
                }
            }
        }

        /// <summary>
        /// 要素内データの選択(0:Vector 1:Polygon 2over:InnerPolygon)
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">2D平面</param>
        /// <returns>選択データ(0:Vector 1:Polygon)</returns>
        private int pickSelect(PointD pickPos, FACE3D face)
        {
            Line3D vec3D = getVector();
            double vl = vec3D.length(pickPos, face);
            Point3D mp = null;
            int n = 1;
            if (0 < mPolygon.mPolygon.Count)
                mp = mPolygon.nearPoint(pickPos, 0, face);
            else {
                double length = double.MaxValue;
                for (int i = 0; i <mInnerPolygon.Count; i++) {
                    mp = mInnerPolygon[i].nearPoint(pickPos, 0, face);
                    if (mp.isNaN())
                        continue;
                    double l = mp.toPoint(face).length(pickPos);
                    if (l < length) {
                        length = l;
                        n = i + 2;
                    }
                }
            }
            if (mp == null || vec3D.length(pickPos, face) < mp.toPoint(face).length(pickPos)) {
                return 0;
            } else {
                return n;
            }
        }

        /// <summary>
        /// 押出ベクトルの表示データ
        /// </summary>
        /// <returns></returns>
        private Line3D getVector()
        {
            Point3D sp;
            if (0 < mPolygon.mPolygon.Count)
                sp = mPolygon.toPoint3D(0);
            else
                sp = mInnerPolygon[0].toPoint3D(0);
            return new Line3D(sp, sp + mVector);
        }

        /// <summary>
        /// 2D平面上の交点
        /// </summary>
        /// <param name="primutive">対象要素</param>
        /// <param name="pos">指定位置</param>
        /// <param name="face">2D平面</param>
        /// <returns>2D交点</returns>
        public override Point3D? intersection(Primitive primutive, PointD pos, FACE3D face)
        {
            return null;
        }

        /// <summary>
        /// 固有データを文字列配列に変換
        /// </summary>
        /// <returns>文字列配列リスト</returns>
        public override List<string[]> toDataList()
        {
            List<string[]> datas = new List<string[]>();
            datas.Add(toDataListBase());
            for (int i = 0; i < mInnerPolygon.Count; i++)
                datas.Add(toDataList(mInnerPolygon[i], i));

            return datas;
        }

        /// <summary>
        /// 押出データの文字列変換
        /// </summary>
        /// <returns>文字配列</returns>
        private string[] toDataListBase()
        {
            bool multi = mPolygon.IsMultiType();
            List<string> dataList = new List<string>() {
                "ExtrusionData", "Vector", mVector.x.ToString(), mVector.y.ToString(), mVector.z.ToString(),
                "Close", mEdgeDisp.ToString(),
                "Loop", mLoop.ToString(),
                "Cp", mPolygon.mCp.x.ToString(), mPolygon.mCp.y.ToString(), mPolygon.mCp.z.ToString(),
                "U", mPolygon.mU.x.ToString(), mPolygon.mU.y.ToString(), mPolygon.mU.z.ToString(),
                "V", mPolygon.mV.x.ToString(), mPolygon.mV.y.ToString(), mPolygon.mV.z.ToString(),
                "Size", mPolygon.mPolygon.Count.ToString(),
                "Multi", multi.ToString(),
            };
            for (int i = 0; i < mPolygon.mPolygon.Count; i++) {
                dataList.Add(mPolygon.mPolygon[i].x.ToString());
                dataList.Add(mPolygon.mPolygon[i].y.ToString());
                if (multi)
                    dataList.Add(mPolygon.mPolygon[i].type.ToString());
            }
            return dataList.ToArray();
        }


        /// <summary>
        /// 追加ポリゴンデータを文字列に変換
        /// </summary>
        /// <param name="polygon">ポリゴン</param>
        /// <param name="count">ポリゴンNo</param>
        /// <returns>文字列配列</returns>
        private string[] toDataList(Polygon3D polygon, int count)
        {
            List<string> dataList;
            bool multi = polygon.IsMultiType();
            dataList = new List<string>() {
                "ExtrusionData" + count.ToString(),
                "Cp", polygon.mCp.x.ToString(), polygon.mCp.y.ToString(), polygon.mCp.z.ToString(),
                "U", polygon.mU.x.ToString(), polygon.mU.y.ToString(), polygon.mU.z.ToString(),
                "V", polygon.mV.x.ToString(), polygon.mV.y.ToString(), polygon.mV.z.ToString(),
                "Size", polygon.mPolygon.Count.ToString(),
                "Multi", multi.ToString(),
                };
            for (int i = 0; i < polygon.mPolygon.Count; i++) {
                dataList.Add(polygon.mPolygon[i].x.ToString());
                dataList.Add(polygon.mPolygon[i].y.ToString());
                if (multi)
                    dataList.Add(polygon.mPolygon[i].type.ToString());
            }
            return dataList.ToArray();
        }


        /// <summary>
        /// 文字列配列から固有データを設定
        /// </summary>
        /// <param name="dataList">文字列配列リスト</param>
        /// <param name="sp">文字列配列位置</param>
        /// <returns>文字列配列位置</returns>
        public override int setDataList(List<string[]>dataList, int sp)
        {
            try {
                while (sp < dataList.Count) {
                    string[] list = dataList[sp];
                    if (0 == list.Length || list[0].IndexOf("ExtrusionData") < 0)
                        break;
                    string np = list[0].Substring("ExtrusionData".Length);
                    if (np.Length == 0)
                        setDataListBase(list);
                    else {
                        mInnerPolygon.Add(getPolygonDataList(list));
                    }
                    sp++;
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine($"Extrusion setDataList {e.ToString()}");
            }
            return sp;
        }

        /// <summary>
        /// 押出のベースデータを文字列配列から取り込む
        /// </summary>
        /// <param name="list">文字列配列</param>
        private void setDataListBase(string[] list)
        {
            int ival;
            double val;
            bool bval;
            int i = 1;
            int count;
            bool multi = false;
            while (i < list.Length) {
                if (list[i] == "Vector") {
                    mVector.x = double.TryParse(list[++i], out val) ? val : 0;
                    mVector.y = double.TryParse(list[++i], out val) ? val : 0;
                    mVector.z = double.TryParse(list[++i], out val) ? val : 0;
                } else if (list[i] == "Close") {
                    mEdgeDisp = bool.TryParse(list[++i], out bval) ? bval : true;
                } else if (list[i] == "Loop") {
                    mLoop = bool.TryParse(list[++i], out bval) ? bval : true;
                } else if (list[i] == "Cp") {
                    Point3D p = new Point3D();
                    p.x = double.TryParse(list[++i], out val) ? val : 0;
                    p.y = double.TryParse(list[++i], out val) ? val : 0;
                    p.z = double.TryParse(list[++i], out val) ? val : 0;
                    mPolygon.mCp = p;
                } else if (list[i] == "U") {
                    Point3D p = new Point3D();
                    p.x = double.TryParse(list[++i], out val) ? val : 0;
                    p.y = double.TryParse(list[++i], out val) ? val : 0;
                    p.z = double.TryParse(list[++i], out val) ? val : 0;
                    mPolygon.mU = p;
                } else if (list[i] == "V") {
                    Point3D p = new Point3D();
                    p.x = double.TryParse(list[++i], out val) ? val : 0;
                    p.y = double.TryParse(list[++i], out val) ? val : 0;
                    p.z = double.TryParse(list[++i], out val) ? val : 0;
                    mPolygon.mV = p;
                } else if (list[i] == "Size") {
                    count = int.TryParse(list[++i], out ival) ? ival : 0;
                } else if (list[i] == "Multi") {
                    multi = bool.TryParse(list[++i], out bval) ? bval : false;
                } else if (ylib.IsNumberString(list[i])) {
                    PointD p = new PointD();
                    p.x = double.TryParse(list[i], out val) ? val : 0;
                    p.y = double.TryParse(list[++i], out val) ? val : 0;
                    if (multi)
                        p.type = int.TryParse(list[++i], out ival) ? ival : 0;
                    mPolygon.mPolygon.Add(p);
                }
                i++;
            }
        }

        /// <summary>
        /// 追加ポリゴンデータを文字列配列から取り込む
        /// </summary>
        /// <param name="list">文字列配列</param>
        /// <returns>ポリゴンデータ</returns>
        private Polygon3D getPolygonDataList(string[] list)
        {
            Polygon3D polygon = new Polygon3D();
            int ival;
            double val;
            bool bval;
            int i = 1;
            int count;
            bool multi = false;
            while (i < list.Length) {
                if (list[i] == "Cp") {
                    Point3D p = new Point3D();
                    p.x = double.TryParse(list[++i], out val) ? val : 0;
                    p.y = double.TryParse(list[++i], out val) ? val : 0;
                    p.z = double.TryParse(list[++i], out val) ? val : 0;
                    polygon.mCp = p;
                } else if (list[i] == "U") {
                    Point3D p = new Point3D();
                    p.x = double.TryParse(list[++i], out val) ? val : 0;
                    p.y = double.TryParse(list[++i], out val) ? val : 0;
                    p.z = double.TryParse(list[++i], out val) ? val : 0;
                    polygon.mU = p;
                } else if (list[i] == "V") {
                    Point3D p = new Point3D();
                    p.x = double.TryParse(list[++i], out val) ? val : 0;
                    p.y = double.TryParse(list[++i], out val) ? val : 0;
                    p.z = double.TryParse(list[++i], out val) ? val : 0;
                    polygon.mV = p;
                } else if (list[i] == "Size") {
                    count = int.TryParse(list[++i], out ival) ? ival : 0;
                } else if (list[i] == "Multi") {
                    multi = bool.TryParse(list[++i], out bval) ? bval : false;
                } else {
                    PointD p = new PointD();
                    p.x = double.TryParse(list[i], out val) ? val : 0;
                    p.y = double.TryParse(list[++i], out val) ? val : 0;
                    if (multi)
                        p.type = int.TryParse(list[++i], out ival) ? ival : 0;
                    polygon.mPolygon.Add(p);
                }
                i++;
            }
            return polygon;
        }


        /// <summary>
        /// 固有データ情報
        /// </summary>
        /// <param name="form">書式</param>
        /// <returns>文字列</returns>
        public override string dataInfo(string form)
        {
            string buf = "ExtrusionData: ";
            buf += "Vector " + mVector.ToString(form);
            buf += " Close " + mEdgeDisp;
            buf += "\nPolygon ";
            buf += $"Cp {mPolygon.mCp.x.ToString(form)},{mPolygon.mCp.y.ToString(form)},{mPolygon.mCp.z.ToString(form)},";
            buf += $"U {mPolygon.mU.x.ToString(form)},{mPolygon.mU.y.ToString(form)},{mPolygon.mU.z.ToString(form)},";
            buf += $"V {mPolygon.mV.x.ToString(form)},{mPolygon.mV.y.ToString(form)},{mPolygon.mV.z.ToString(form)},";
            buf += $"Size {mPolygon.mPolygon.Count} ";
            for (int i = 0; i < mPolygon.mPolygon.Count; i++) {
                buf += "," + mPolygon.mPolygon[i].ToString(form);
            }
            return buf;
        }

        /// <summary>
        /// サマリデータ
        /// </summary>
        /// <param name="form">データ書式</param>
        /// <returns>文字列</returns>
        public override string dataSummary(string form = "F2")
        {
            return $"領域:{getArea().ToString(form)}";
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        public override Primitive toCopy()
        {
            ExtrusionPrimitive extrusion = new ExtrusionPrimitive();
            extrusion.copyProperty(this, true, true);
            extrusion.mPolygon = mPolygon.toCopy();
            extrusion.mVector = mVector.toCopy();
            extrusion.mEdgeDisp = mEdgeDisp;
            extrusion.mLoop = mLoop;
            if (0 < mInnerPolygon.Count) {
                extrusion.mInnerPolygon = new List<Polygon3D>();
                foreach (var polygon in mInnerPolygon)
                    extrusion.mInnerPolygon.Add(polygon.toCopy());
            }
            return extrusion;
        }

        /// <summary>
        /// 座標点リストを求める
        /// </summary>
        /// <returns>ポリライン</returns>
        public override Polyline3D toPointList()
        {
            return new Polyline3D();
        }

        /// <summary>
        /// 座標点リストを求める
        /// </summary>
        /// <returns>ポリライン</returns>
        public override Polyline3D getVertexList()
        {
            return new Polyline3D();
        }

        /// <summary>
        /// コマンド出力
        /// </summary>
        /// <returns></returns>
        public override string toCommand()
        {
            return $"extruction ";
        }
    }
}
