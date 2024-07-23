using CoreLib;

namespace Mini3DCad
{
    /// <summary>
    /// 回転体クラス
    /// </summary>
    public class RevolutionPrimitive : Primitive
    {
        public Line3D mCenterLine;
        public Polyline3D mOutLine;
        public double mSa = 0;
        public double mEa = Math.PI * 2;
        public bool mLoop = true;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RevolutionPrimitive()
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="centerLine">回転中心線</param>
        /// <param name="outLine">外形線</param>
        /// <param name="close">閉領域</param>
        /// <param name="face">作成面</param>
        public RevolutionPrimitive(Line3D centerLine, Polyline3D outLine, double divideAngle = Math.PI / 16, bool close = true)
        {
            mPrimitiveId = PrimitiveId.Revolution;
            mCenterLine = centerLine.toCopy();
            mOutLine = outLine.toCopy();
            mDivideAngle = divideAngle;
            mLoop = close;
        }

        /// <summary>
        /// 3D座標リストの作成
        /// </summary>
        public override void createSurfaceData()
        {
            mSurfaceDataList = new List<SurfaceData>();
            SurfaceData surfaceData;
            //  回転座標作成
            List<List<Point3D>> outLines;
            outLines = getCenterLineRotate(mCenterLine, mOutLine.toPoint3D(), mDivideAngle);
            if (mWireFrame) {
                //  ワイヤーフレーム
                for (int i = 0; i < outLines.Count; i++) {
                    surfaceData = new SurfaceData();
                    surfaceData.mVertexList = outLines[i];
                    surfaceData.mDrawType = DRAWTYPE.LINE_STRIP;
                    surfaceData.mFaceColor = mFaceColors[0];
                    mSurfaceDataList.Add(surfaceData);
                    surfaceData = new SurfaceData();
                    surfaceData.mVertexList = new List<Point3D>();
                    if (i < outLines.Count - 1) {
                        for (int j = 0; j < outLines[i].Count; j++) {
                            surfaceData.mVertexList.Add(outLines[i + 1][j]);
                            surfaceData.mVertexList.Add(outLines[i][j]);
                        }
                        surfaceData.mDrawType = DRAWTYPE.LINES;
                        surfaceData.mFaceColor = mFaceColors[0];
                        mSurfaceDataList.Add(surfaceData);
                    }
                }
            } else {
                //  Surfaceの作成
                for (int i = 0; i < outLines.Count - 1; i++) {
                    surfaceData = new SurfaceData();
                    surfaceData.mVertexList = new List<Point3D>();
                    for (int j = 0; j < outLines[i].Count; j++) {
                        surfaceData.mVertexList.Add(outLines[i + 1][j]);
                        surfaceData.mVertexList.Add(outLines[i][j]);
                    }
                    surfaceData.mDrawType = DRAWTYPE.QUAD_STRIP;
                    surfaceData.mFaceColor = mFaceColors[0];
                    surfaceData.reverse(mReverse);
                    mSurfaceDataList.Add(surfaceData);
                }
                //  端面表示
                if (mEdgeDisp && (mEa - mSa < Math.PI * 2)) {
                    Point3D vec0 = outLines[1][0] - outLines[0][0];
                    Polygon3D polygon0 = new Polygon3D(outLines[0]);
                    mSurfaceDataList.Add(createSurfaceData(polygon0, vec0, mDivideAngle, mFaceColors[0]));
                    Point3D vec1 = outLines[^2][0] - outLines[^1][0];
                    Polygon3D polygon1 = new Polygon3D(outLines[^1]);
                    mSurfaceDataList.Add(createSurfaceData(polygon1, vec1, mDivideAngle, mFaceColors[0]));
                }
            }
        }

        /// <summary>
        /// 2D表示用座標リストの作成
        /// </summary>
        public override void createVertexData()
        {
            mVertexList = new List<List<Point3D>>();
            if (mOutlineDisp) {
                mOutlineColors.Clear();
                mOutlineType.Clear();
                mVertexList.Add(mCenterLine.toPoint3D());
                mOutlineColors.Add(System.Windows.Media.Brushes.Green);
                mOutlineType.Add(2);
                mVertexList.Add(mOutLine.toPoint3D(mDivideAngle));
                mOutlineColors.Add(mLineColor);
                mOutlineType.Add(mLineType);
            } else {
                List<List<Point3D>> outLines;
                double divideAngle = mDivideAngle < (Math.PI / 6) ? mDivideAngle * 2 : mDivideAngle;
                outLines = getCenterLineRotate(mCenterLine, mOutLine.toPoint3D(), divideAngle);
                mVertexList.AddRange(outLines);
                for (int i = 0; i < outLines[0].Count; i++) {
                    List<Point3D> plist = new List<Point3D>();
                    for (int j = 0; j < outLines.Count; j++) {
                        plist.Add(outLines[j][i]);
                    }
                    mVertexList.Add(plist);
                }
            }
        }

        /// <summary>
        /// 回転体の外形線作成
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="outline"></param>
        /// <returns></returns>
        private List<List<Point3D>> getCenterLineRotate(Line3D centerline, List<Point3D> outline, double divideAngle)
        {
            List<List<Point3D>> outLines = new List<List<Point3D>>();
            Point3D cp = centerline.mSp;
            Point3D cv = cp.vector(centerline.endPoint());    //  中心線ベクトル
            cp.inverse();
            outline.ForEach(p => p.add(cp));
            cp.inverse();
            double ang = mSa;
            double dang = divideAngle;
            while ((ang - dang) < mEa) {
                if (mEa < ang)
                    ang = mEa;
                List<Point3D> plist = outline.ConvertAll(p => p.toCopy());
                plist.ForEach(p => p.rotate(cv, ang));
                plist.ForEach(p => p.add(cp));
                outLines.Add(plist);
                ang += dang;
            }
            return outLines;
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
                int n = pickSelect(pickPos, face);
                if (n == 0) {
                    mCenterLine.translate(v);
                } else if (n == 1) {
                    mOutLine.translate(v);
                }
            } else {
                mCenterLine.translate(v);
                mOutLine.translate(v);
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
                int n = pickSelect(pickPos, face);
                if (n == 0) {
                    mCenterLine.rotate(cp, ang, face);
                } else if (n == 1) {
                    mOutLine.rotate(cp, ang, face);
                }
            } else {
                mCenterLine.rotate(cp, ang, face);
                mOutLine.rotate(cp, ang, face);
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
                int n = pickSelect(pickPos, face);
                if (n == 0) {
                    mCenterLine.offset(ep - sp);
                } else if (n == 1) {
                    mOutLine.offset(sp, ep);
                }
            } else {
                mOutLine.offset(sp, ep);
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
            Line3D l = new Line3D(sp, ep);
            mCenterLine = l.mirror(mCenterLine, face);
            mCenterLine.reverse();
            mOutLine.mirror(l, face);
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
        /// <param name="scale">拡大率</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">"2D平面</param>
        public override void scale(Point3D cp, double scale, PointD pickPos, FACE3D face)
        {
            if (mOutlineDisp) {
                int n = pickSelect(pickPos, face);
                if (n == 0) {
                    mCenterLine.scale(cp, scale);
                } else if (n == 1) {
                    mOutLine.scale(cp, scale);
                }
            } else {
                mCenterLine.scale(cp, scale);
                mOutLine.scale(cp, scale);
            }
        }

        /// <summary>
        /// ストレッチ
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="arc">円弧変形</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">"2D平面</param>
        public override void stretch(Point3D vec, bool arc, PointD pickPos, FACE3D face)
        {
            if (mOutlineDisp) {
                int n = pickSelect(pickPos, face);
                if (n == 0) {
                    mCenterLine.stretch(vec, new Point3D(pickPos, face));
                } else if (n == 1) {
                    mOutLine.stretch(vec, new Point3D(pickPos, face), arc);
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
            Point3D mp1 = mCenterLine.intersection(pickPos, face);
            Point3D mp2 = mOutLine.nearPoint(pickPos, 0, face);
            if (!mOutLine.onPoint(mp2))
                    mp2 = null;
            if (mp1 == null && mp2 == null) {
                return -1;
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
        /// 固有データを文字列配列に変換
        /// </summary>
        /// <returns>文字列配列</returns>
        public override string[] toDataList()
        {
            bool multi = mOutLine.IsMultiType();
            List<string> dataList = new List<string>() {
                "RevolutionData",
                "StartAngle", mSa.ToString(),
                "EndAngle", mEa.ToString(),
                "Close", mEdgeDisp.ToString(),
                "CenterLineSp", mCenterLine.mSp.x.ToString(), mCenterLine.mSp.y.ToString(), mCenterLine.mSp.z.ToString(),
                "CenterLineV", mCenterLine.mV.x.ToString(), mCenterLine.mV.y.ToString(), mCenterLine.mV.z.ToString(),
                "OutLineCp", mOutLine.mCp.x.ToString(), mOutLine.mCp.y.ToString(), mOutLine.mCp.z.ToString(),
                "OutLineU", mOutLine.mU.x.ToString(), mOutLine.mU.y.ToString(), mOutLine.mU.z.ToString(),
                "OutLineV", mOutLine.mV.x.ToString(), mOutLine.mV.y.ToString(), mOutLine.mV.z.ToString(),
                "OutLineSize", mOutLine.mPolyline.Count.ToString(),
                "Multi", multi.ToString(),
                "OutLine"
            };
            for (int i = 0; i < mOutLine.mPolyline.Count; i++) {
                dataList.Add(mOutLine.mPolyline[i].x.ToString());
                dataList.Add(mOutLine.mPolyline[i].y.ToString());
                if (multi)
                    dataList.Add(mOutLine.mPolyline[i].type.ToString());
            }
            return dataList.ToArray();
        }

        /// <summary>
        /// 文字列配列から固有データを設定
        /// </summary>
        /// <param name="list">文字列配列</param>
        public override void setDataList(string[] list)
        {
            if (0 == list.Length || list[0] != "RevolutionData")
                return;
            try {
                mCenterLine = new Line3D();
                mOutLine = new Polyline3D();
                int ival;
                double val;
                bool bval;
                int i = 1;
                int count = 0;
                bool multi = false;
                while (i < list.Length) {
                    if (list[i] == "StartAngle") {
                        mSa = double.TryParse(list[++i], out val) ? val : 0;
                    } else if (list[i] == "EndAngle") {
                        mEa = double.TryParse(list[++i], out val) ? val : 0;
                    } else if (list[i] == "Close") {
                        mEdgeDisp = bool.TryParse(list[++i], out bval) ? bval : true;
                    } else if (list[i] == "CenterLineSp") {
                        Point3D p = new Point3D();
                        p.x = double.TryParse(list[++i], out val) ? val : 0;
                        p.y = double.TryParse(list[++i], out val) ? val : 0;
                        p.z = double.TryParse(list[++i], out val) ? val : 0;
                        mCenterLine.mSp = p;
                    } else if (list[i] == "CenterLineV") {
                        Point3D p = new Point3D();
                        p.x = double.TryParse(list[++i], out val) ? val : 0;
                        p.y = double.TryParse(list[++i], out val) ? val : 0;
                        p.z = double.TryParse(list[++i], out val) ? val : 0;
                        mCenterLine.mV = p;
                    } else if (list[i] == "OutLineCp") {
                        Point3D p = new Point3D();
                        p.x = double.TryParse(list[++i], out val) ? val : 0;
                        p.y = double.TryParse(list[++i], out val) ? val : 0;
                        p.z = double.TryParse(list[++i], out val) ? val : 0;
                        mOutLine.mCp = p;
                    } else if (list[i] == "OutLineU") {
                        Point3D p = new Point3D();
                        p.x = double.TryParse(list[++i], out val) ? val : 0;
                        p.y = double.TryParse(list[++i], out val) ? val : 0;
                        p.z = double.TryParse(list[++i], out val) ? val : 0;
                        mOutLine.mU = p;
                    } else if (list[i] == "OutLineV") {
                        Point3D p = new Point3D();
                        p.x = double.TryParse(list[++i], out val) ? val : 0;
                        p.y = double.TryParse(list[++i], out val) ? val : 0;
                        p.z = double.TryParse(list[++i], out val) ? val : 0;
                        mOutLine.mV = p;
                    } else if (list[i] == "OutLineSize") {
                        count = int.TryParse(list[++i], out ival) ? ival : 0;
                    } else if (list[i] == "Multi") {
                        multi = bool.TryParse(list[++i], out bval) ? bval : false;
                    } else if (list[i] == "OutLine") {
                        for (int j = 0; j < count; j++) {
                            PointD p = new PointD();
                            p.x = double.TryParse(list[++i], out val) ? val : 0;
                            p.y = double.TryParse(list[++i], out val) ? val : 0;
                            if (multi)
                                p.type = int.TryParse(list[++i], out ival) ? ival : 0;
                            mOutLine.mPolyline.Add(p);
                        }
                    }
                    i++;
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine($"Revolution setDataList {e.ToString()}");
            }
        }

        /// <summary>
        /// 固有データ情報
        /// </summary>
        /// <param name="form">書式</param>
        /// <returns>文字列</returns>
        public override string dataInfo(string form)
        {
            string buf = "RevolutionData: ";
            buf += " StartAngle " + mSa.ToString(form);
            buf += " EndAngle " + mEa.ToString(form);
            buf += " DivideAngle " + mDivideAngle.ToString(form);
            buf += " Close " + mEdgeDisp.ToString();
            buf += $"\nCenterLine Sp {mCenterLine.mSp.ToString(form)} V {mCenterLine.mV.ToString(form)}";
            buf += $"\nOutLine Size {mOutLine.mPolyline.Count} ";
            for (int i = 0; i < mOutLine.mPolyline.Count; i++) {
                buf += "," + mOutLine.mPolyline[i].ToString(form);
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
            RevolutionPrimitive revolusion = new RevolutionPrimitive();
            revolusion.copyProperty(this, true, true);
            revolusion.mSa = mSa;
            revolusion.mEa = mEa;
            revolusion.mDivideAngle = mDivideAngle;
            revolusion.mCenterLine = mCenterLine.toCopy();
            revolusion.mOutLine = mOutLine.toCopy();
            return revolusion;
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
            return $"revolution ";
        }
    }
}
