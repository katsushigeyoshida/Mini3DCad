using CoreLib;

namespace Mini3DCad
{
    public class BlendPolylinePrimitive : Primitive
    {
        public Polyline3D mPolyline1;
        public Polyline3D mPolyline2;
        public int mCount = 0;          //  アウトラインデータカウント(ファイル出力用)

        public BlendPolylinePrimitive()
        {

        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="polyline1">ポリライン</param>
        /// <param name="polyline2">ポリライン</param>
        /// <param name="divAng">円弧分割角度</param>
        public BlendPolylinePrimitive(Polyline3D polyline1, Polyline3D polyline2, double divAng = 0)
        {
            mPrimitiveId = PrimitiveId.BlendPolyline;
            mPolyline1 = polyline1.toCopy();
            mPolyline2 = polyline2.toCopy();
            mPolyline1.squeeze();
            mPolyline2.squeeze();
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
                mSurfaceDataList = createSurfcaeDataList();
            }
        }

        /// <summary>
        /// サーフェスデータの作成
        /// </summary>
        /// <returns>サーフェスデータリスト</returns>
        private List<SurfaceData> createSurfcaeDataList()
        {
            if (mPolyline1 == null || mPolyline2 == null)
                return null;
            List<SurfaceData> surfaceDataList = new List<SurfaceData>();
            SurfaceData surfaceData = new SurfaceData();
            List<Point3D> plist = crateVertexData(0);
            surfaceData.mVertexList = plist;
            surfaceData.mDrawType = DRAWTYPE.QUAD_STRIP;
            surfaceData.mFaceColor = mFaceColors[0];
            surfaceData.reverse(mReverse);
            surfaceDataList.Add(surfaceData);

            return surfaceDataList;
        }

        /// <summary>
        /// ワイヤーフレーム表示データ作成
        /// </summary>
        /// <returns>サーフェスデータリスト</returns>
        private List<SurfaceData> createWireFrameDataList()
        {
            if (mPolyline1 == null || mPolyline2 == null)
                return null;
            List<SurfaceData>  surfaceDataList = new List<SurfaceData>();
            SurfaceData surfaceData = new SurfaceData();
            List< Point3D > plist = crateVertexData(1);
            List<Point3D> edgelist = new List<Point3D>();
            for (int  i = 0;  i < plist.Count - 3;  i += 2) {
                edgelist.Add(plist[i]);
                edgelist.Add(plist[i + 2]);
                edgelist.Add(plist[i + 1]);
                edgelist.Add(plist[i + 3]);
            }
            plist.AddRange(edgelist);
            surfaceData.mVertexList = plist;
            surfaceData.mDrawType = DRAWTYPE.LINES;
            surfaceData.mFaceColor = mFaceColors[0];
            surfaceDataList.Add(surfaceData);

            return surfaceDataList;
        }

        /// <summary>
        /// 2D表示用座標リストの作成
        /// </summary>
        public override void createVertexData()
        {
            if (mOutlineDisp) {
                mOutlineColors.Clear();
                mOutlineType.Clear();
                mOutlineColors.Add(mLineColor);
                mOutlineType.Add(mLineType);
            }
            mVertexList = new List<List<Point3D>>();
            mVertexList.Add(mPolyline1.toPoint3D(mDivideAngle / 2));
            mVertexList.Add(mPolyline2.toPoint3D(mDivideAngle / 2));
            if (!mOutlineDisp) {
                List<Point3D> plist = crateVertexData(1);
                for (int i = 0; i < plist.Count; i += 2) {
                    List<Point3D> line = new List<Point3D>() {
                        plist[i], plist[i + 1]
                    };
                    mVertexList.Add(line);
                }
            }
        }

        /// <summary>
        /// 座標点リストの作成
        /// </summary>
        /// <param name="st">開始位置</param>
        /// <returns>座標点リスト</returns>
        private List<Point3D> crateVertexData(int st = 0)
        {
            if (mPolyline1 == null || mPolyline2 == null)
                return null;
            List<List<Point3D>> vertexList = new List<List<Point3D>>();
            Line3D line1, line2;
            Arc3D arc1, arc2;
            Point3D p1 = mPolyline1.toPoint3D(0);
            Point3D p2 = mPolyline2.toPoint3D(0);
            List<Point3D> plist = new List<Point3D>() { p1, p2};
            for (int i = 0, j = 0; i < mPolyline1.mPolyline.Count - 1 && j < mPolyline2.mPolyline.Count - 1; i++, j++) {
                if (i < mPolyline1.mPolyline.Count - 2 && mPolyline1.mPolyline[i + 1].type == 1) {
                    line1 = null;
                    arc1 = new Arc3D(mPolyline1.toPoint3D(i), mPolyline1.toPoint3D(i + 1), mPolyline1.toPoint3D(i + 2));
                    i++;
                } else {
                    line1 = new Line3D(mPolyline1.toPoint3D(i), mPolyline1.toPoint3D(i + 1));
                    arc1 = null;
                }
                if (j < mPolyline2.mPolyline.Count - 2 && mPolyline2.mPolyline[j + 1].type == 1) {
                    line2 = null;
                    arc2 = new Arc3D(mPolyline2.toPoint3D(j), mPolyline2.toPoint3D(j + 1), mPolyline2.toPoint3D(j + 2));
                    j++;
                } else {
                    line2 = new Line3D(mPolyline2.toPoint3D(j), mPolyline2.toPoint3D(j + 1));
                    arc2 = null;
                }
                if (line1 != null && line2 != null) {
                    plist.AddRange(createVertexData(line1, line2));
                } else if (line1 != null && arc2 != null) {
                    plist.AddRange(createVertexData(line1, arc2, false, st));
                } else if (line2 != null && arc1 != null) {
                    plist.AddRange(createVertexData(line2, arc1, false, st));
                } else if (arc1 != null && arc2 != null) {
                    plist.AddRange(createVertexData(arc1, arc2));
                }
            }
            return plist;
        }

        /// <summary>
        /// 線分同士のブレンド
        /// </summary>
        /// <param name="line1">線分</param>
        /// <param name="line2">線分</param>
        /// <returns>座標点リスト</returns>
        private List<Point3D> createVertexData(Line3D line1, Line3D line2)
        {
            List<Point3D> plist = new List<Point3D>() { line1.endPoint(), line2.endPoint() };
            return plist;
        }

        /// <summary>
        /// 線分と円弧のブレンド
        /// </summary>
        /// <param name="line">線分</param>
        /// <param name="arc">円弧</param>
        /// <param name="reverse">反転</param>
        /// <param name="st">開始位置</param>
        /// <returns>座標点リスト</returns>
        private List<Point3D> createVertexData(Line3D line, Arc3D arc, bool reverse = false, int st = 0)
        {
            List<Point3D> arcPlist = arc.toPoint3D(mDivideAngle);
            if (!arc.mCcw) arcPlist.Reverse();
            List<Point3D> linePlist = line.toPoint3D(arcPlist.Count - 1);
            List<Point3D> plist = new List<Point3D>();
            for (int i = st; i < arcPlist.Count; i++) {
                if (reverse) {
                    plist.Add(arcPlist[i]);
                    plist.Add(linePlist[i]);
                } else {
                    plist.Add(linePlist[i]);
                    plist.Add(arcPlist[i]);
                }
            }
            return plist;
        }

        /// <summary>
        /// 円弧同士のブレンド
        /// </summary>
        /// <param name="arc1">円弧</param>
        /// <param name="arc2">円弧</param>
        /// <returns>座標点リスト</returns>
        private List<Point3D> createVertexData(Arc3D arc1, Arc3D arc2)
        {
            int n = 1;
            if (arc1.mOpenAngle < arc2.mOpenAngle)
                n = (int)(arc2.mOpenAngle / mDivideAngle) + 1;
            else
                n = (int)(arc1.mOpenAngle / mDivideAngle) + 1;
            List<Point3D> arc1Plist = arc1.toPoint3D(n);
            if (!arc1.mCcw) arc1Plist.Reverse();
            List<Point3D> arc2Plist = arc2.toPoint3D(n);
            List<Point3D> plist = new List<Point3D>();
            for (int i = 1; i < arc1Plist.Count && i < arc2Plist.Count; i++) {
                plist.Add(arc1Plist[i]);
                plist.Add(arc2Plist[i]);
            }
            return plist;
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
                    mPolyline1.translate(v);
                } else if (select == 1) {
                    mPolyline2.translate(v);
                }
            } else {
                mPolyline1.translate(v);
                mPolyline2.translate(v);
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
                    mPolyline1.rotate(cp, ang, face);
                } else if (select == 1) {
                    mPolyline2.rotate(cp, ang, face);
                }
            } else {
                mPolyline1.rotate(cp, ang, face);
                mPolyline2.rotate(cp, ang, face);
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
                    mPolyline1.offset(sp, ep);
                } else if (select == 1) {
                    mPolyline2.offset(sp, ep);
                }
            } else {
                mPolyline1.offset(sp, ep);
                mPolyline2.offset(sp, ep);
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
            if (mOutlineDisp) {
                int select = pickSelect(pickPos, face);
                if (select == 0) {
                    mPolyline1.mirror(new Line3D(sp, ep), face);
                } else if (select == 1) {
                    mPolyline2.mirror(new Line3D(sp, ep), face);
                }
            } else {
                mPolyline1.mirror(new Line3D(sp, ep), face);
                mPolyline2.mirror(new Line3D(sp, ep), face);
            }
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
            if (mOutlineDisp) {
                int select = pickSelect(pickPos, face);
                if (select == 0) {
                    mPolyline1.trim(sp, ep);
                } else if (select == 1) {
                    mPolyline1.trim(sp, ep);
                }
            }
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
                    mPolyline1.scale(cp, scale);
                } else if (select == 1) {
                    mPolyline2.scale(cp, scale);
                }
            } else {
                mPolyline1.scale(cp, scale);
                mPolyline2.scale(cp, scale);
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
                if (select == 0) {
                    mPolyline1.stretch(vec, new Point3D(pickPos, face), arc);
                } else if (select == 1) {
                    mPolyline2.stretch(vec, new Point3D(pickPos, face), arc);
                }
            }
        }

        /// <summary>
        /// 要素内データの選択(-1:なし,0:Polygon1,1:Polygon2)
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">2D平面</param>
        /// <returns>選択データ</returns>
        private int pickSelect(PointD pickPos, FACE3D face)
        {
            Point3D mp1 = mPolyline1.nearPoint(pickPos, 0, face);
            Point3D mp2 = mPolyline2.nearPoint(pickPos, 0, face);
            if (mp1 == null && mp2 == null) {
            } else if (mp1 != null && mp2 == null) {
                return 0;
            } else if (mp1 == null && mp2 != null) {
                return 1;
            } else if (mp1.toPoint(face).length(pickPos) < mp2.toPoint(face).length(pickPos)) {
                return 0;
            } else {
                return 1;
            }
            return -1;
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
            List<string[]> datasList = new List<string[]>() { 
                toDataList(mPolyline1, 1),
                toDataList(mPolyline2, 2),
            };
            return datasList;
        }

        /// <summary>
        /// ポリラインデータを文字列に変換
        /// </summary>
        /// <param name="polyline">ポリライン</param>
        /// <param name="count">ポリラインNo</param>
        /// <returns>文字列配列</returns>
        private string[] toDataList(Polyline3D polyline, int count)
        {
            List<string> dataList;
            bool multi = polyline.IsMultiType();
            dataList = new List<string>() {
                    "BlendPolylineData" + count.ToString(),
                    "Cp", polyline.mCp.x.ToString(), polyline.mCp.y.ToString(), polyline.mCp.z.ToString(),
                    "U", polyline.mU.x.ToString(), polyline.mU.y.ToString(), polyline.mU.z.ToString(),
                    "V", polyline.mV.x.ToString(), polyline.mV.y.ToString(), polyline.mV.z.ToString(),
                    "Size", polyline.mPolyline.Count.ToString(),
                    "Multi", multi.ToString(),
                };
            for (int i = 0; i < polyline.mPolyline.Count; i++) {
                dataList.Add(polyline.mPolyline[i].x.ToString());
                dataList.Add(polyline.mPolyline[i].y.ToString());
                if (multi)
                    dataList.Add(polyline.mPolyline[i].type.ToString());
            }
            return dataList.ToArray();
        }

        /// <summary>
        /// 文字列配列から固有データを設定
        /// </summary>
        /// <param name="dataList">文字列配列リスト</param>
        /// <param name="sp">文字列配列位置</param>
        /// <returns>文字列配列位置</returns>
        public override int setDataList(List<string[]> dataList, int sp)
        {
            try {
                while (sp < dataList.Count) {
                    string[] list = dataList[sp];
                    if (0 == list.Length)
                        break;
                    if (list[0] == "BlendPolylineData1") {
                        mPolyline1 = getPolylineDataList(list);
                        mPolyline1.squeeze();
                    } else if (list[0] == "BlendPolylineData2") {
                        mPolyline2 = getPolylineDataList(list);
                        mPolyline2.squeeze();
                    } else
                        break;
                    sp++;
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine($"BlendPolyline setDataList {e.ToString()}");
            }
            return sp;
        }

        /// <summary>
        /// 文字データからポリラインデータの取得
        /// </summary>
        /// <param name="list">文字配列</param>
        /// <returns>ポリライン</returns>
        private Polyline3D getPolylineDataList(string[] list)
        {
            Polyline3D polyline = new Polyline3D();
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
                    polyline.mCp = p;
                } else if (list[i] == "U") {
                    Point3D p = new Point3D();
                    p.x = double.TryParse(list[++i], out val) ? val : 0;
                    p.y = double.TryParse(list[++i], out val) ? val : 0;
                    p.z = double.TryParse(list[++i], out val) ? val : 0;
                    polyline.mU = p;
                } else if (list[i] == "V") {
                    Point3D p = new Point3D();
                    p.x = double.TryParse(list[++i], out val) ? val : 0;
                    p.y = double.TryParse(list[++i], out val) ? val : 0;
                    p.z = double.TryParse(list[++i], out val) ? val : 0;
                    polyline.mV = p;
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
                    polyline.mPolyline.Add(p);
                }
                i++;
            }
            return polyline;
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
            buf += $"Cp {mPolyline1.mCp.x.ToString(form)},{mPolyline1.mCp.y.ToString(form)},{mPolyline1.mCp.z.ToString(form)},";
            buf += $"U {mPolyline1.mU.x.ToString(form)},{mPolyline1.mU.y.ToString(form)},{mPolyline1.mU.z.ToString(form)},";
            buf += $"V {mPolyline1.mV.x.ToString(form)},{mPolyline1.mV.y.ToString(form)},{mPolyline1.mV.z.ToString(form)},";
            buf += $"Size {mPolyline1.mPolyline.Count} ";
            for (int i = 0; i < mPolyline1.mPolyline.Count; i++) {
                buf += "," + mPolyline1.mPolyline[i].ToString(form);
            }
            buf += "\nPolygon2 ";
            buf += $"Cp {mPolyline2.mCp.x.ToString(form)},{mPolyline2.mCp.y.ToString(form)},{mPolyline2.mCp.z.ToString(form)},";
            buf += $"U {mPolyline2.mU.x.ToString(form)},{mPolyline2.mU.y.ToString(form)},{mPolyline2.mU.z.ToString(form)},";
            buf += $"V {mPolyline2.mV.x.ToString(form)},{mPolyline2.mV.y.ToString(form)},{mPolyline2.mV.z.ToString(form)},";
            buf += $"Size {mPolyline2.mPolyline.Count} ";
            for (int i = 0; i < mPolyline2.mPolyline.Count; i++) {
                buf += "," + mPolyline2.mPolyline[i].ToString(form);
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
            BlendPolylinePrimitive blend = new BlendPolylinePrimitive();
            blend.copyProperty(this, true, true);
            blend.mPolyline1 = mPolyline1.toCopy();
            blend.mPolyline2 = mPolyline2.toCopy();
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
