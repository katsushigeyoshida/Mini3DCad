using CoreLib;

namespace Mini3DCad
{

    /// <summary>
    /// スィープ
    /// </summary>
    public class SweepPrimitive : Primitive
    {
        public Polyline3D mOutLine1;
        public Polyline3D mOutLine2;
        public double mSa = 0;
        public double mEa = Math.PI * 2;
        public bool mLoop = true;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SweepPrimitive()
        {
            mPrimitiveId = PrimitiveId.Sweep;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="outline1">外形線</param>
        /// <param name="outline2">外形線</param>
        /// <param name="color">色</param>
        /// <param name="close">閉領域</param>
        /// <param name="face">作成面</param>
        public SweepPrimitive(Polyline3D outline1, Polyline3D outline2,
            double divideAngle = Math.PI / 9, bool close = true)
        {
            mPrimitiveId = PrimitiveId.Sweep;
            mOutLine1 = outline1.toCopy();
            mOutLine2 = outline2.toCopy();
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
            if (!directChk(mOutLine1, mOutLine2))
                mOutLine2.mPolyline.Reverse();
            outLines = rotateOutlines(mOutLine1, mOutLine2, mDivideAngle);
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
                    int i1 = (i + 1) % outLines.Count;
                    for (int j = 0; j < outLines[i].Count; j++) {
                        surfaceData.mVertexList.Add(outLines[i1][j]);
                        surfaceData.mVertexList.Add(outLines[i][j]);
                    }
                    surfaceData.mDrawType = DRAWTYPE.LINES;
                    surfaceData.mFaceColor = mFaceColors[0];
                    mSurfaceDataList.Add(surfaceData);
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
                if (mEdgeDisp) {
                    surfaceData = new SurfaceData();
                    surfaceData.mVertexList = new List<Point3D>();
                    for (int j = 0; j < outLines[^1].Count; j++) {
                        surfaceData.mVertexList.Add(outLines[0][j]);
                        surfaceData.mVertexList.Add(outLines[^1][j]);
                    }
                    surfaceData.mDrawType = DRAWTYPE.QUAD_STRIP;
                    surfaceData.mFaceColor = mFaceColors[0];
                    surfaceData.reverse(mReverse);
                    mSurfaceDataList.Add(surfaceData);
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
                mVertexList.Add(mOutLine1.toPoint3D(mDivideAngle));
                mOutlineColors.Add(mLineColor);
                mOutlineType.Add(mLineType);
                mVertexList.Add(mOutLine2.toPoint3D(mDivideAngle));
                mOutlineColors.Add(mLineColor);
                mOutlineType.Add(mLineType);
            } else {
                List<List<Point3D>> outLines;
                if (!directChk(mOutLine1, mOutLine2))
                    mOutLine2.mPolyline.Reverse();
                double divideAngle = mDivideAngle < (Math.PI / 6) ? mDivideAngle * 2 : mDivideAngle;
                outLines = rotateOutlines(mOutLine1, mOutLine2, divideAngle);
                mVertexList.AddRange(outLines);
                for (int j = 0; j < outLines[0].Count; j++) {
                    List<Point3D> plist = new List<Point3D>();
                    for (int i = 0; i < outLines.Count; i++) {
                        plist.Add(outLines[i][j].toCopy());
                    }
                    mVertexList.Add(plist);
                }
            }
        }

        /// <summary>
        /// 外形線同士の方向チェック
        /// </summary>
        /// <param name="outline1">外形線1</param>
        /// <param name="outline2">外形線2</param>
        /// <param name="face">作成2D平面</param>
        /// <returns></returns>
        private bool directChk(Polyline3D outline1, Polyline3D outline2)
        {
            Point3D sp1 = outline1.toFirstPoint3D();
            Point3D ep1 = outline1.toLastPoint3D();
            Point3D sp2 = outline2.toFirstPoint3D();
            Point3D ep2 = outline2.toLastPoint3D();
            Line3D l1 = new Line3D(sp1, sp2);
            Line3D l2 = new Line3D(ep1, ep2);
            Point3D ip = l1.intersection(l2);
            if (ip == null)
                return true;
            if (l1.onPoint(ip))
                return false;
            else
                return true;
        }

        /// <summary>
        /// 回転外形線の作成
        /// </summary>
        /// <param name="outline1">外形線1</param>
        /// <param name="outline2">外形線2</param>
        /// <returns>回転外形線リスト</returns>
        private List<List<Point3D>> rotateOutlines(Polyline3D outline1, Polyline3D outline2, double divideAngle)
        {
            List<List<Point3D>> outLines = new List<List<Point3D>>();
            //  中心線リスト
            (List<Line3D> centerlines, List<Line3D> outlines) = getCenterlines(outline1, outline2);
            double ang = mSa;
            double dang = divideAngle;
            while (ang < mEa) {
                List<Point3D> plist = new List<Point3D>();
                for (int i = 0; i < centerlines.Count; i++) {
                    Point3D cp = centerlines[i].mSp;
                    Point3D cv = centerlines[i].mV;
                    Point3D sp = outlines[i].mSp.toCopy();
                    Point3D ep = outlines[i].endPoint();
                    sp.sub(cp);
                    ep.sub(cp);
                    sp.rotate(cv, ang);
                    ep.rotate(cv, ang);
                    sp.add(cp);
                    ep.add(cp);
                    plist.Add(sp);
                    plist.Add(ep);
                }
                ang += dang;
                outLines.Add(plist);
            }
            return outLines;
        }

        /// <summary>
        /// 中心線リストの作成
        /// </summary>
        /// <param name="outline1">外形線1</param>
        /// <param name="outline2">外形線2</param>
        /// <returns>中心線リスト、外形線リスト</returns>
        private (List<Line3D> centerlines, List<Line3D> outlines) getCenterlines(Polyline3D outline1, Polyline3D outline2)
        {
            List<Line3D> centerlines = new List<Line3D>();
            List<Line3D> outlines = new List<Line3D>();
            int lineCount = Math.Min(outline1.mPolyline.Count, outline2.mPolyline.Count);
            for (int i = 0; i < lineCount - 1; i++) {
                Line3D l1 = new Line3D(outline1.toPoint3D(i), outline1.toPoint3D(i + 1));
                Line3D l2 = new Line3D(outline2.toPoint3D(i), outline2.toPoint3D(i + 1));
                (Line3D centerline, Line3D outline) = getCenterline(l1, l2);
                centerlines.Add(centerline);
                outlines.Add(outline);
            }
            return (centerlines, outlines);
        }

        /// <summary>
        /// ２線の中心線を求める
        /// </summary>
        /// <param name="l1">外形線分1</param>
        /// <param name="l2">外形線分2</param>
        /// <returns>中心線</returns>
        private (Line3D centerline, Line3D outline) getCenterline(Line3D l1, Line3D l2)
        {
            Point3D cp1, sp, cp2, ep;
            if (l1.mV.angle(l2.mV) < Math.PI / 180) {
                (cp1, sp) = getStartCenter(l1, l2);
                l1.reverse();
                l2.reverse();
                (cp2, ep) = getStartCenter(l1, l2);
                l1.reverse();
                l2.reverse();
            } else {
                Line3D cl1 = new Line3D(l1.mSp, l2.mSp);
                cp1 = cl1.centerPoint();
                sp = l1.mSp.toCopy();
                Line3D cl2 = new Line3D(l1.endPoint(), l2.endPoint());
                cp2 = cl2.centerPoint();
                ep = l1.endPoint();
            }
            return (new Line3D(cp1, cp2), new Line3D(sp, ep));
        }

        /// <summary>
        /// 外形線の中心を求める
        /// </summary>
        /// <param name="l1"></param>
        /// <param name="l2"></param>
        /// <returns></returns>
        private (Point3D cp, Point3D sp) getStartCenter(Line3D l1, Line3D l2)
        {
            Point3D ip1 = l1.intersection(l2.mSp);
            Point3D ip2 = l2.intersection(ip1);
            if (!l1.onPoint(ip1) || !l2.onPoint(ip2)) {
                ip2 = l2.intersection(l1.mSp);
                ip1 = l1.intersection(ip2);
            }
            Line3D l = new Line3D(ip1, ip2);
            Point3D cp = l.centerPoint();
            Point3D sp = ip1;
            return (cp, sp);
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
                    mOutLine1.translate(v);
                } else if (n == 1) {
                    mOutLine2.translate(v);
                }
            } else {
                mOutLine1.translate(v);
                mOutLine2.translate(v);
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
                    mOutLine1.rotate(cp, ang, face);
                } else if (n == 1) {
                    mOutLine2.rotate(cp, ang, face);
                }
            } else {
                mOutLine1.rotate(cp, ang, face);
                mOutLine2.rotate(cp, ang, face);
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
            double d = ep.length(sp);
            if (mOutlineDisp) {
                int n = pickSelect(pickPos, face);
                if (n == 0) {
                    mOutLine1.offset(d);
                } else if (n == 1) {
                    mOutLine2.offset(d);
                }
            } else {
                mOutLine1.offset(d);
                mOutLine2.offset(d);
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
            mOutLine1.mirror(new Line3D(sp, ep), face);
            mOutLine2.mirror(new Line3D(sp, ep), face);
            YLib.Swap(ref mOutLine1, ref mOutLine2);
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
                    mOutLine1.scale(cp, scale);
                } else if (n == 1) {
                    mOutLine2.scale(cp, scale);
                }
            } else {
                mOutLine1.scale(cp, scale);
                mOutLine2.scale(cp, scale);
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
                    mOutLine1.stretch(vec, new Point3D(pickPos, face), arc);
                } else if (n == 1) {
                    mOutLine2.stretch(vec, new Point3D(pickPos, face), arc);
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
            Point3D mp1 = mOutLine1.nearPoint(pickPos, 0, face);
            Point3D mp2 = mOutLine2.nearPoint(pickPos, 0, face);
            if (!mOutLine1.onPoint(mp1))
                mp1 = null;
            if (!mOutLine2.onPoint(mp2))
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
        /// <returns>文字列配列</returns>
        public override string[] toDataList()
        {
            bool multi1 = mOutLine1.IsMultiType();
            List<string> dataList = new List<string>() {
                "SweepData",
                "StartAngle", mSa.ToString(),
                "EndAngle", mEa.ToString(),
                "Close", mEdgeDisp.ToString(),
                "OutLine1Cp", mOutLine1.mCp.x.ToString(), mOutLine1.mCp.y.ToString(), mOutLine1.mCp.z.ToString(),
                "OutLine1U", mOutLine1.mU.x.ToString(), mOutLine1.mU.y.ToString(), mOutLine1.mU.z.ToString(),
                "OutLine1V", mOutLine1.mV.x.ToString(), mOutLine1.mV.y.ToString(), mOutLine1.mV.z.ToString(),
                "OutLine1Size", mOutLine1.mPolyline.Count.ToString(),
                "Multi1", multi1.ToString(),
                "OutLine1"
            };
            for (int i = 0; i < mOutLine1.mPolyline.Count; i++) {
                dataList.Add(mOutLine1.mPolyline[i].x.ToString());
                dataList.Add(mOutLine1.mPolyline[i].y.ToString());
                if (multi1)
                    dataList.Add(mOutLine1.mPolyline[i].type.ToString());
            }
            bool multi2 = mOutLine2.IsMultiType();
            List<string> buf = new List<string>() {
                "OutLine2Cp", mOutLine2.mCp.x.ToString(), mOutLine2.mCp.y.ToString(), mOutLine2.mCp.z.ToString(),
                "OutLine2U", mOutLine2.mU.x.ToString(), mOutLine2.mU.y.ToString(), mOutLine2.mU.z.ToString(),
                "OutLine2V", mOutLine2.mV.x.ToString(), mOutLine2.mV.y.ToString(), mOutLine2.mV.z.ToString(),
                "OutLine2Size", mOutLine2.mPolyline.Count.ToString(),
                "Multi2", multi2.ToString(),
                "OutLine2"
            };
            dataList.AddRange(buf);
            for (int i = 0; i < mOutLine2.mPolyline.Count; i++) {
                dataList.Add(mOutLine2.mPolyline[i].x.ToString());
                dataList.Add(mOutLine2.mPolyline[i].y.ToString());
                if (multi2)
                    dataList.Add(mOutLine2.mPolyline[i].type.ToString());
            }
            return dataList.ToArray();
        }

        /// <summary>
        /// 文字列配列から固有データを設定
        /// </summary>
        /// <param name="list">文字列配列</param>
        public override void setDataList(string[] list)
        {
            if (0 == list.Length || list[0] != "SweepData")
                return;
            try {
                mOutLine1 = new Polyline3D();
                mOutLine2 = new Polyline3D();
                int ival;
                double val;
                bool bval;
                int i = 1;
                int count = 0;
                bool multi1 = false;
                bool multi2 = false;
                while (i < list.Length) {
                    if (list[i] == "StartAngle") {
                        mSa = double.TryParse(list[++i], out val) ? val : 0;
                    } else if (list[i] == "EndAngle") {
                        mEa = double.TryParse(list[++i], out val) ? val : 0;
                    } else if (list[i] == "Close") {
                        mEdgeDisp = bool.TryParse(list[++i], out bval) ? bval : true;
                    } else if (list[i] == "OutLine1Cp") {
                        Point3D p = new Point3D();
                        p.x = double.TryParse(list[++i], out val) ? val : 0;
                        p.y = double.TryParse(list[++i], out val) ? val : 0;
                        p.z = double.TryParse(list[++i], out val) ? val : 0;
                        mOutLine1.mCp = p;
                    } else if (list[i] == "OutLine1U") {
                        Point3D p = new Point3D();
                        p.x = double.TryParse(list[++i], out val) ? val : 0;
                        p.y = double.TryParse(list[++i], out val) ? val : 0;
                        p.z = double.TryParse(list[++i], out val) ? val : 0;
                        mOutLine1.mU = p;
                    } else if (list[i] == "OutLine1V") {
                        Point3D p = new Point3D();
                        p.x = double.TryParse(list[++i], out val) ? val : 0;
                        p.y = double.TryParse(list[++i], out val) ? val : 0;
                        p.z = double.TryParse(list[++i], out val) ? val : 0;
                        mOutLine1.mV = p;
                    } else if (list[i] == "OutLine1Size") {
                        count = int.TryParse(list[++i], out ival) ? ival : 0;
                    } else if (list[i] == "Multi1") {
                        multi1 = bool.TryParse(list[++i], out bval) ? bval : false;
                    } else if (list[i] == "OutLine1") {
                        for (int j = 0; j < count; j++) {
                            PointD p = new PointD();
                            p.x = double.TryParse(list[++i], out val) ? val : 0;
                            p.y = double.TryParse(list[++i], out val) ? val : 0;
                            if (multi1)
                                p.type = int.TryParse(list[++i], out ival) ? ival : 0;
                            mOutLine1.mPolyline.Add(p);
                        }
                    } else if (list[i] == "OutLine2Cp") {
                        Point3D p = new Point3D();
                        p.x = double.TryParse(list[++i], out val) ? val : 0;
                        p.y = double.TryParse(list[++i], out val) ? val : 0;
                        p.z = double.TryParse(list[++i], out val) ? val : 0;
                        mOutLine2.mCp = p;
                    } else if (list[i] == "OutLine2U") {
                        Point3D p = new Point3D();
                        p.x = double.TryParse(list[++i], out val) ? val : 0;
                        p.y = double.TryParse(list[++i], out val) ? val : 0;
                        p.z = double.TryParse(list[++i], out val) ? val : 0;
                        mOutLine2.mU = p;
                    } else if (list[i] == "OutLine2V") {
                        Point3D p = new Point3D();
                        p.x = double.TryParse(list[++i], out val) ? val : 0;
                        p.y = double.TryParse(list[++i], out val) ? val : 0;
                        p.z = double.TryParse(list[++i], out val) ? val : 0;
                        mOutLine2.mV = p;
                    } else if (list[i] == "OutLine2Size") {
                        count = int.TryParse(list[++i], out ival) ? ival : 0;
                    } else if (list[i] == "Multi2") {
                        multi2 = bool.TryParse(list[++i], out bval) ? bval : false;
                    } else if (list[i] == "OutLine2") {
                        for (int j = 0; j < count; j++) {
                            PointD p = new PointD();
                            p.x = double.TryParse(list[++i], out val) ? val : 0;
                            p.y = double.TryParse(list[++i], out val) ? val : 0;
                            if (multi2)
                                p.type = int.TryParse(list[++i], out ival) ? ival : 0;
                            mOutLine2.mPolyline.Add(p);
                        }
                    }
                    i++;
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine($"Sweep setDataList {e.ToString()}");
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
            buf += $"\nOutLine1 Cp {mOutLine1.mCp.ToString(form)} U {mOutLine1.mU.ToString(form)} V {mOutLine1.mV.ToString(form)}";
            buf += $" size {mOutLine1.mPolyline.Count} ";
            for (int i = 0; i < mOutLine1.mPolyline.Count; i++) {
                buf += "," + mOutLine1.mPolyline[i].ToString(form);
            }
            buf += $"\nOutLine2 Cp {mOutLine2.mCp.ToString(form)} U {mOutLine2.mU.ToString(form)} V {mOutLine2.mV.ToString(form)}";
            buf += $" size {mOutLine2.mPolyline.Count} ";
            for (int i = 0; i < mOutLine2.mPolyline.Count; i++) {
                buf += "," + mOutLine2.mPolyline[i].ToString(form);
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
            SweepPrimitive revolusion = new SweepPrimitive();
            revolusion.copyProperty(this, true, true);
            revolusion.mSa = mSa;
            revolusion.mEa = mEa;
            revolusion.mDivideAngle = mDivideAngle;
            revolusion.mOutLine1 = mOutLine1.toCopy();
            revolusion.mOutLine2 = mOutLine2.toCopy();
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
            return $"sweep ";
        }
    }
}
