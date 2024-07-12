using CoreLib;

namespace Mini3DCad
{

    /// <summary>
    /// ブレンド
    /// </summary>
    public class BlendPrimitive : Primitive
    {
        public Polygon3D mPolygon1;
        public Polygon3D mPolygon2;
        public int mCount = 0;          //  アウトラインデータカウント(ファイル出力用)

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BlendPrimitive()
        {

        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="polygon1">ポリゴン</param>
        /// <param name="polygon2">ポリゴン</param>
        /// <param name="divAng">円弧変換角度</param>
        public BlendPrimitive(Polygon3D polygon1, Polygon3D polygon2, double divAng = 0)
        {
            mPrimitiveId = PrimitiveId.Blend;
            mPolygon1 = polygon1.toCopy();
            mPolygon2 = polygon2.toCopy();
        }

        /// <summary>
        /// 3D座標リストの作成(三角形の集合)
        /// </summary>
        public override void createSurfaceData()
        {
            mSurfaceDataList = new List<SurfaceData>();
            SurfaceData surfaceData;
            if (mWireFrame) {
                //  ワイヤフレーム表示
                mSurfaceDataList = createWireFrameDataList();
            } else {
                //  サーフェース表示
                mSurfaceDataList = createSurfceDataList();
            }
        }

        /// <summary>
        /// ワイヤーフレーム表示データの作成
        /// </summary>
        /// <returns></returns>
        private List<SurfaceData> createWireFrameDataList()
        {
            List<SurfaceData> wireFrameDataList = new List<SurfaceData>();
            SurfaceData surfaceData;
            Polyline3D polyline1 = new Polyline3D(mPolygon1);
            surfaceData = new SurfaceData();
            surfaceData.mVertexList = polyline1.toPoint3D(mDivideAngle);
            surfaceData.mDrawType = DRAWTYPE.LINE_STRIP;
            surfaceData.mFaceColor = mFaceColors[0];
            wireFrameDataList.Add(surfaceData);
            Polyline3D polyline2 = new Polyline3D(mPolygon2);
            surfaceData = new SurfaceData();
            surfaceData.mVertexList = polyline2.toPoint3D(mDivideAngle);
            surfaceData.mDrawType = DRAWTYPE.LINE_STRIP;
            surfaceData.mFaceColor = mFaceColors[0];
            wireFrameDataList.Add(surfaceData);
            //  側面
            surfaceData = new SurfaceData();
            List<Point3D> plist = new List<Point3D>();
            for (int i = 0; i < mPolygon1.mPolygon.Count; i++) {
                Point3D p1 = polyline1.toPoint3D(i);
                Point3D p2 = polyline2.toPoint3D(i);
                plist.Add(p1);
                plist.Add(p2);
            }
            surfaceData.mVertexList = plist;
            surfaceData.mDrawType = DRAWTYPE.LINES;
            surfaceData.mFaceColor = mFaceColors[0];
            wireFrameDataList.Add(surfaceData);
            return wireFrameDataList;
        }

        /// <summary>
        /// サーフェース表示データの作成
        /// </summary>
        /// <returns></returns>
        private List<SurfaceData> createSurfceDataList()
        {
            List<SurfaceData> surfaceDataList = new List<SurfaceData>();
            SurfaceData surfaceData;
            Point3D normal = mPolygon1.getNormalLine();
            Point3D vector = mPolygon2.toPoint3D(0) - mPolygon1.toPoint3D(0);
            bool wise = (Math.PI / 2) > normal.angle(vector);
            if (mEdgeDisp) {
                //  1面(端面)
                surfaceData = new SurfaceData();
                (surfaceData.mVertexList, bool reverse1) = mPolygon1.cnvTriangles(mDivideAngle);
                Point3D v1 = surfaceData.mVertexList[0].getNormal(surfaceData.mVertexList[1], surfaceData.mVertexList[2]);
                if (v1.angle(vector) < Math.PI / 2)
                    surfaceData.mVertexList.Reverse();
                surfaceData.mDrawType = DRAWTYPE.TRIANGLES;
                surfaceData.mFaceColor = mFaceColors[0];
                surfaceData.reverse(mReverse);
                surfaceDataList.Add(surfaceData);
                //  2面(端面)
                surfaceData = new SurfaceData();
                (surfaceData.mVertexList, bool reverse2) = mPolygon2.cnvTriangles(mDivideAngle);
                Point3D v2 = surfaceData.mVertexList[0].getNormal(surfaceData.mVertexList[1], surfaceData.mVertexList[2]);
                if (v2.angle(vector) > Math.PI / 2)
                    surfaceData.mVertexList.Reverse();
                surfaceData.mDrawType = DRAWTYPE.TRIANGLES;
                surfaceData.mFaceColor = mFaceColors[0];
                surfaceData.reverse(mReverse);
                surfaceDataList.Add(surfaceData);
            }
            //  側面
            surfaceData = new SurfaceData();
            surfaceData.mVertexList = new List<Point3D>();
            List<Point3D> outline1 = mPolygon1.toPoint3D(mDivideAngle);
            List<Point3D> outline2 = mPolygon2.toPoint3D(mDivideAngle);
            for (int i = 0; i <= outline1.Count; i++) {
                int ii = i % outline1.Count;
                if (!wise) {
                    surfaceData.mVertexList.Add(outline1[ii]);
                    surfaceData.mVertexList.Add(outline2[ii]);
                } else {
                    surfaceData.mVertexList.Add(outline2[ii]);
                    surfaceData.mVertexList.Add(outline1[ii]);
                }
            }
            surfaceData.mDrawType = DRAWTYPE.QUAD_STRIP;
            surfaceData.mFaceColor = mFaceColors[0];
            surfaceData.reverse(mReverse);
            surfaceDataList.Add(surfaceData);
            return surfaceDataList;
        }

        /// <summary>
        /// 2D表示用座標リストの作成
        /// </summary>
        public override void createVertexData()
        {
            if (mOutlineDisp) {
                mVertexList = crateOutlineVertexData();
            } else {
                mVertexList = crateBlendVertexData();
            }
        }

        /// <summary>
        /// 2Dのアウトライン表示データ作成
        /// </summary>
        /// <returns></returns>
        private List<List<Point3D>> crateOutlineVertexData()
        {
            List<List<Point3D>>  vertexList = new List<List<Point3D>>();
            //  3D
            mOutlineColors.Clear();
            mOutlineType.Clear();
            mOutlineColors.Add(mLineColor);
            mOutlineType.Add(mLineType);
            vertexList.Add(mPolygon1.toPoint3D(mDivideAngle, true));
            vertexList.Add(mPolygon2.toPoint3D(mDivideAngle, true));
            return vertexList;
        }

        /// <summary>
        /// 2D表示データ作成
        /// </summary>
        /// <returns></returns>
        private List<List<Point3D>> crateBlendVertexData()
        {
            List<List<Point3D>> vertexList = new List<List<Point3D>>();
            //  1面(端面)
            Polygon3D polygon1 = mPolygon1.toCopy();
            vertexList.Add(polygon1.toPoint3D(mDivideAngle, true));
            if (!mSurfaceVertex) {
                vertexList.Add(polygon1.toPoint3D(mDivideAngle, true));
            } else {
                //  Debug(Surface表示確認)
                vertexList.AddRange(getVertexList(polygon1));
            }
            //  2面(端面)
            Polygon3D polygon2 = mPolygon2.toCopy();
            vertexList.Add(polygon2.toPoint3D(mDivideAngle, true));
            if (!mSurfaceVertex) {
                vertexList.Add(polygon2.toPoint3D(mDivideAngle, true));
            } else {
                //  Debug(Surface表示確認)
                vertexList.AddRange(getVertexList(polygon2));
            }
            //  側面
            for (int i = 0; i < polygon1.mPolygon.Count; i++) {
                Point3D p1 = polygon1.toPoint3D(i);
                Point3D p2 = polygon2.toPoint3D(i);
                List<Point3D> plist = new List<Point3D>() { p1, p2 };
                vertexList.Add(plist);
            }
            return vertexList;
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">2D平面</param>
        public override void translate(Point3D v, PointD pickPos, FACE3D face)
        {
            if (mOutlineDisp) {
                int select = pickSelect(pickPos, face);
                if (select == 0) {
                    mPolygon1.translate(v);
                } else if (select == 1) {
                    mPolygon2.translate(v);
                }
            } else {
                mPolygon1.translate(v);
                mPolygon2.translate(v);
            }
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
            if (mOutlineDisp) {
                int select = pickSelect(pickPos, face);
                if (select == 0) {
                    mPolygon1.rotate(cp, ang, face);
                } else if (select == 1) {
                    mPolygon2.rotate(cp, ang, face);
                }
            } else {
                mPolygon1.rotate(cp, ang, face);
                mPolygon2.rotate(cp, ang, face);
            }
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
            if (mOutlineDisp) {
                int select = pickSelect(pickPos, face);
                if (select == 0) {
                    mPolygon1.offset(sp, ep);
                } else if (select == 1) {
                    mPolygon2.offset(sp, ep);
                }
            } else {
                mPolygon1.offset(sp, ep);
                mPolygon2.offset(sp, ep);
            }
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
            mPolygon1.mirror(new Line3D(sp, ep), face);
            mPolygon2.mirror(new Line3D(sp, ep), face);
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
                    mPolygon1.scale(cp, scale);
                } else if (select == 1) {
                    mPolygon2.scale(cp, scale);
                }
            } else {
                mPolygon1.scale(cp, scale);
                mPolygon2.scale(cp, scale);
            }
        }

        /// <summary>
        /// ストレッチ
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="arc">円弧変形</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">"D平面</param>
        public override void stretch(Point3D vec, bool arc, PointD pickPos, FACE3D face)
        {
            if (mOutlineDisp) {
                int select = pickSelect(pickPos, face);
                if ( select == 0) {
                    mPolygon1.stretch(vec, new Point3D(pickPos, face), arc);
                } else if (select == 1) {
                    mPolygon2.stretch(vec, new Point3D(pickPos, face), arc);
                }
            }
        }

        /// <summary>
        /// 要素内データの選択
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">2D平面</param>
        /// <returns>選択データ</returns>
        private int pickSelect(PointD pickPos, FACE3D face)
        {
            PointD mp1 = mPolygon1.nearPoint(pickPos, 0, face);
            PointD mp2 = mPolygon2.nearPoint(pickPos, 0, face);
            if (mp1 == null && mp2 == null) {
            } else if (mp1 != null && mp2 == null) {
                return 0;
            } else if (mp1 == null && mp2 != null) {
                return 1;
            } else if (mp1.length(pickPos) < mp2.length(pickPos)) {
                return 0;
            } else {
                return 1;
            }
            return -1;
        }

        /// <summary>
        /// 固有データを文字列配列に変換
        /// </summary>
        /// <returns>文字列配列</returns>
        public override string[] toDataList()
        {
            List<string> dataList;
            Polygon3D polygon = mCount == 0 ? mPolygon1 : mPolygon2;
            bool multi = polygon.IsMultiType();
            dataList = new List<string>() {
                    "BlendData" + (mCount + 1).ToString(),
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
            mCount++;
            if (1 < mCount) mCount = 0;
            return dataList.ToArray();
        }

        /// <summary>
        /// 文字列配列から固有データを設定
        /// </summary>
        /// <param name="list">文字列配列</param>
        public override void setDataList(string[] list)
        {
            if (0 == list.Length)
                return;
            try {
                if (list[0] == "BlendData1") {
                    mPolygon1 = getPolygonDataList(list);
                } else if (list[0] == "BlendData2") {
                    mPolygon2 = getPolygonDataList(list);
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine($"Extrusion setDataList {e.ToString()}");
            }
        }

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
            string buf = "BlendData: ";
            buf += "\nPolygon1 ";
            buf += $"Cp {mPolygon1.mCp.x.ToString(form)},{mPolygon1.mCp.y.ToString(form)},{mPolygon1.mCp.z.ToString(form)},";
            buf += $"U {mPolygon1.mU.x.ToString(form)},{mPolygon1.mU.y.ToString(form)},{mPolygon1.mU.z.ToString(form)},";
            buf += $"V {mPolygon1.mV.x.ToString(form)},{mPolygon1.mV.y.ToString(form)},{mPolygon1.mV.z.ToString(form)},";
            buf += $"Size {mPolygon1.mPolygon.Count} ";
            for (int i = 0; i < mPolygon1.mPolygon.Count; i++) {
                buf += "," + mPolygon1.mPolygon[i].ToString(form);
            }
            buf += "\nPolygon2 ";
            buf += $"Cp {mPolygon2.mCp.x.ToString(form)},{mPolygon2.mCp.y.ToString(form)},{mPolygon2.mCp.z.ToString(form)},";
            buf += $"U {mPolygon2.mU.x.ToString(form)},{mPolygon2.mU.y.ToString(form)},{mPolygon2.mU.z.ToString(form)},";
            buf += $"V {mPolygon2.mV.x.ToString(form)},{mPolygon2.mV.y.ToString(form)},{mPolygon2.mV.z.ToString(form)},";
            buf += $"Size {mPolygon2.mPolygon.Count} ";
            for (int i = 0; i < mPolygon2.mPolygon.Count; i++) {
                buf += "," + mPolygon2.mPolygon[i].ToString(form);
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
            BlendPrimitive blend = new BlendPrimitive();
            blend.copyProperty(this, true, true);
            blend.mPolygon1 = mPolygon1.toCopy();
            blend.mPolygon2 = mPolygon2.toCopy();
            return blend;
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
