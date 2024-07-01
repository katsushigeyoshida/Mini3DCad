using CoreLib;

namespace Mini3DCad
{

    /// <summary>
    /// 押出プリミティブ
    /// </summary>
    public class ExtrusionPrimitive : Primitive
    {
        public Polygon3D mPolygon;
        public Point3D mVector;
        public FACE3D mSrcFace;
        public bool mLoop = true;       //  Polyline(false)とPolygon(true)

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ExtrusionPrimitive()
        {
            mPolygon = new Polygon3D();
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
        public ExtrusionPrimitive(Polygon3D polygon, Point3D v, bool loop = true)
        {
            mPrimitiveId = PrimitiveId.Extrusion;
            mPolygon = polygon.toCopy();
            mPolygon.squeeze();
            mVector = v;
            mLoop = loop;
        }

        /// <summary>
        /// 3D座標リストの作成
        /// </summary>
        public override void createSurfaceData()
        {
            mSurfaceDataList = new List<SurfaceData>();
            SurfaceData surfaceData;
            if (mWireFrame) {
                //  ワイヤフレーム表示
                Polyline3D polyline1 = new Polyline3D(mPolygon);
                surfaceData = new SurfaceData();
                surfaceData.mVertexList = polyline1.toPoint3D(mDivideAngle);
                surfaceData.mDrawType = DRAWTYPE.LINE_STRIP;
                surfaceData.mFaceColor = mFaceColors[0];
                mSurfaceDataList.Add(surfaceData);
                Polyline3D polyline2 = polyline1.toCopy();
                polyline2.translate(mVector);
                surfaceData = new SurfaceData();
                surfaceData.mVertexList = polyline2.toPoint3D(mDivideAngle);
                surfaceData.mDrawType = DRAWTYPE.LINE_STRIP;
                surfaceData.mFaceColor = mFaceColors[0];
                mSurfaceDataList.Add(surfaceData);
                //  側面
                surfaceData = new SurfaceData();
                List<Point3D> plist = new List<Point3D>();
                for (int i = 0; i < mPolygon.mPolygon.Count; i++) {
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
            } else {
                //  サーフェース表示
                Point3D normal = mPolygon.getNormalLine();
                bool wise = (Math.PI / 2) > normal.angle(mVector);
                if (mEdgeDisp) {
                    //  1面(端面)
                    mSurfaceDataList.Add(createSurfaceData(mPolygon, mVector, mDivideAngle, mFaceColors[0]));
                    //  2面(端面)
                    Polygon3D polygon = mPolygon.toCopy();
                    polygon.mCp += mVector;
                    Point3D vector = mVector.toCopy();
                    vector.inverse();
                    mSurfaceDataList.Add(createSurfaceData(polygon, vector, mDivideAngle, mFaceColors[0]));
                }
                //  側面
                surfaceData = new SurfaceData();
                surfaceData.mVertexList = new List<Point3D>();
                List<Point3D> outline = mPolygon.toPoint3D(mDivideAngle);
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
                mVertexList.Add(mPolygon.toPoint3D(mDivideAngle, mLoop));
                mOutlineColors.Add(mFaceColors[0]);
                mOutlineType.Add(mLineType);
                mVertexList.Add(getVector().toPoint3D());
                mOutlineColors.Add(System.Windows.Media.Brushes.Green);
                mOutlineType.Add(1);
            } else {
                //  1面(端面)
                Polygon3D polygon1 = mPolygon.toCopy();
                mVertexList.Add(polygon1.toPoint3D(mDivideAngle, mLoop));
                if (!mSurfaceVertex) {
                    mVertexList.Add(polygon1.toPoint3D(mDivideAngle, mLoop));
                } else {
                    //  Debug(Surface表示確認)
                    mVertexList.AddRange(getVertexList(polygon1));
                }
                //  2面(端面)
                Polygon3D polygon2 = mPolygon.toCopy();
                polygon2.translate(mVector);
                mVertexList.Add(polygon2.toPoint3D(mDivideAngle, mLoop));
                if (!mSurfaceVertex) {
                    mVertexList.Add(polygon2.toPoint3D(mDivideAngle, mLoop));
                } else {
                    //  Debug(Surface表示確認)
                    mVertexList.AddRange(getVertexList(polygon2));
                }
                //  側面
                for (int i = 0; i < polygon1.mPolygon.Count; i++) {
                    Point3D p1 = polygon1.toPoint3D(i);
                    Point3D p2 = p1.toCopy();
                    p2.add(mVector);
                    List<Point3D> plist = new List<Point3D>() { p1, p2 };
                    mVertexList.Add(plist);
                }
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
            mVector.length(mVector.length() * scale);
            mPolygon.scale(cp, scale);
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
                if (select == 1) {
                    Line3D vec3D = getVector();
                    vec3D.stretch(vec, new Point3D(pickPos, face));
                    mVector = vec3D.mV.toCopy();
                } else {
                    mPolygon.stretch(vec, new Point3D(pickPos, face), arc);
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
            Line3D vec3D = getVector();
            double vl = vec3D.length(pickPos, face);
            PointD mp = mPolygon.nearPoint(pickPos, 0, face);
            if (mp == null || vec3D.length(pickPos, face) < mp.length(pickPos)) {
                return 0;
            } else {
                return 1;
            }
        }

        /// <summary>
        /// 押出ベクトルの表示データ
        /// </summary>
        /// <returns></returns>
        private Line3D getVector()
        {
            Point3D sp = mPolygon.toPoint3D(0);
            return new Line3D(sp, sp + mVector);
        }

        /// <summary>
        /// 固有データを文字列配列に変換
        /// </summary>
        /// <returns>文字列配列</returns>
        public override string[] toDataList()
        {
            bool multi = mPolygon.IsMultiType();
            List<string> dataList = new List<string>() {
                "ExtrusionData", "Vector", mVector.x.ToString(), mVector.y.ToString(), mVector.z.ToString(),
                "Close", mEdgeDisp.ToString(),
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
        /// 文字列配列から固有データを設定
        /// </summary>
        /// <param name="list">文字列配列</param>
        public override void setDataList(string[] list)
        {
            if (0 == list.Length || list[0] != "ExtrusionData")
                return;
            try {
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
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine($"Extrusion setDataList {e.ToString()}");
            }
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
