using CoreLib;

namespace Mini3DCad
{

    /// <summary>
    /// 線分プリミティブ
    /// </summary>
    public class LinePrimitive : Primitive
    {
        public Line3D mLine;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LinePrimitive()
        {
            mPrimitiveId = PrimitiveId.Line;
            mLine = new Line3D();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="line">線分</param>
        /// <param name="face">作成面</param>
        public LinePrimitive(Line3D line)
        {
            mPrimitiveId = PrimitiveId.Line;
            mLine = line.toCopy();
        }

        /// <summary>
        /// 3D座標(Surface)リストの作成
        /// </summary>
        /// <returns>座標リスト</returns>
        public override void createSurfaceData()
        {
            mSurfaceDataList = new List<SurfaceData>();
            SurfaceData surfaceData = new SurfaceData();
            surfaceData.mVertexList = mLine.toPoint3D();
            surfaceData.mDrawType = DRAWTYPE.LINE_STRIP;
            surfaceData.mFaceColor = mLineColor;
            mSurfaceDataList.Add(surfaceData);
        }

        /// <summary>
        /// 2D表示用座標リストの作成
        /// </summary>
        public override void createVertexData()
        {
            mVertexList = new List<List<Point3D>>();
            mVertexList.Add(mLine.toPoint3D());
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public override void translate(Point3D v, PointD pickPos, FACE3D face)
        {
            mLine.translate(v);
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">操作面</param>
        public override void rotate(Point3D cp, double ang, PointD pickPos, FACE3D face)
        {
            mLine.rotate(cp, ang, face);
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public override void offset(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            Point3D v = ep - sp;
            mLine.offset(v);
        }

        /// <summary>
        /// 線分の座標を反転する
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public override void mirror(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            Line3D l = new Line3D(sp, ep);
            mLine = l.mirror(mLine, face);
        }

        /// <summary>
        /// トリム
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        /// <param name="outline">3Dデータと外形線の作成</param>
        public override void trim(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            mLine.trim(sp, ep);
        }

        /// <summary>
        /// 拡大縮小
        /// </summary>
        /// <param name="cp">拡大中心</param>
        /// <param name="scale">倍率</param>
        /// <param name="face">2D平面</param>
        public override void scale(Point3D cp, double scale, PointD pickPos, FACE3D face)
        {
            mLine.scale(cp, scale);
        }

        /// <summary>
        /// ストレッチ
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">2D平面</param>
        public override void stretch(Point3D vec, bool arc, PointD pickPos, FACE3D face)
        {
            Point3D pos = new Point3D(pickPos, face);
            mLine.stretch(vec, pos);
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
            PointD? ip = null;
            LineD line = mLine.toLineD(face);
            if (primutive.mPrimitiveId == PrimitiveId.Point) {
                PointD p = ((PointPrimitive)primutive).mPoint.toPoint(face);
                ip = line.intersection(p);
            } else if (primutive.mPrimitiveId == PrimitiveId.Line) {
                LineD l = ((LinePrimitive)primutive).mLine.toLineD(face);
                ip = line.intersection(l);
            } else if (primutive.mPrimitiveId == PrimitiveId.Arc) {
                EllipseD eli = ((ArcPrimitive)primutive).mArc.toEllipseD(face);
                List<PointD> iplist = eli.intersection(line);
                if (iplist != null && 0 < iplist.Count)
                    ip = iplist.MinBy(p => p.length(pos));
            } else if (primutive.mPrimitiveId == PrimitiveId.Polyline) {
                PolylineD polyline = ((PolylinePrimitive)primutive).mPolyline.toPolylineD(face);
                List<PointD> iplist = polyline.intersection(line);
                if (iplist != null && 0 < iplist.Count)
                    ip = iplist.MinBy(p => p.length(pos));
            } else if (primutive.mPrimitiveId == PrimitiveId.Polygon) {
                PolygonD polygon = ((PolygonPrimitive)primutive).mPolygon.toPolygonD(face);
                List<PointD> iplist = polygon.intersection(line);
                if (iplist != null && 0 < iplist.Count)
                    ip = iplist.MinBy(p => p.length(pos));
            }
            if (ip != null)
                return mLine.intersection(ip, face);
            else
                return null;
        }

        /// <summary>
        /// 固有データを文字列配列に変換
        /// </summary>
        /// <returns>文字列配列</returns>
        public override string[] toDataList()
        {
            List<string> dataList = new List<string>() {
                "LineData",
                "Sp", mLine.mSp.x.ToString(), mLine.mSp.y.ToString(), mLine.mSp.z.ToString(),
                "V", mLine.mV.x.ToString(), mLine.mV.y.ToString(), mLine.mV.z.ToString(),
            };
            return dataList.ToArray();
        }

        /// <summary>
        /// 文字列配列から固有データを設定
        /// </summary>
        /// <param name="list">文字列配列</param>
        public override void setDataList(string[] list)
        {
            if (0 == list.Length || list[0] != "LineData")
                return;
            try {
                double val;
                for (int i = 1; i < list.Length; i++) {
                    if (list[i] == "Sp") {
                        mLine.mSp.x = double.TryParse(list[++i], out val) ? val : 0;
                        mLine.mSp.y = double.TryParse(list[++i], out val) ? val : 0;
                        mLine.mSp.z = double.TryParse(list[++i], out val) ? val : 0;
                    } else if (list[i] == "Ep") {
                        Point3D ep = new Point3D();
                        ep.x = double.TryParse(list[++i], out val) ? val : 0;
                        ep.y = double.TryParse(list[++i], out val) ? val : 0;
                        ep.z = double.TryParse(list[++i], out val) ? val : 0;
                        mLine.mV = ep - mLine.mSp;
                    } else if (list[i] == "V") {
                        mLine.mV.x = double.TryParse(list[++i], out val) ? val : 0;
                        mLine.mV.y = double.TryParse(list[++i], out val) ? val : 0;
                        mLine.mV.z = double.TryParse(list[++i], out val) ? val : 0;
                    }
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine($"Line setDataList {e.ToString()}");
            }
        }

        /// <summary>
        /// 固有データ情報
        /// </summary>
        /// <param name="form">書式</param>
        /// <returns>文字列</returns>
        public override string dataInfo(string form)
        {
            return "LineData:" +
                " Sp " + mLine.mSp.ToString(form) +
                " V " + mLine.mV.ToString(form);
        }

        /// <summary>
        /// サマリデータ
        /// </summary>
        /// <param name="form">データ書式</param>
        /// <returns>文字列</returns>
        public override string dataSummary(string form = "F2")
        {
            return $"座標:{mLine.mSp.ToString(form)},{mLine.endPoint().ToString(form)}";
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        public override Primitive toCopy()
        {
            LinePrimitive line = new LinePrimitive();
            line.copyProperty(this, true, true);
            line.mLine = mLine.toCopy();
            return line;
        }

        /// <summary>
        /// 座標点リストの取得
        /// </summary>
        /// <returns>ポリライン</returns>
        public override Polyline3D toPointList()
        {
            return new Polyline3D(mLine.toPoint3D());
        }

        /// <summary>
        /// 座標点リストを求める
        /// </summary>
        /// <returns>ポリライン</returns>
        public override Polyline3D getVertexList()
        {
            return new Polyline3D(mLine.toPoint3D());
        }

        /// <summary>
        /// コマンド出力
        /// </summary>
        /// <returns></returns>
        public override string toCommand()
        {
            Point3D sp = mLine.mSp;
            Point3D ep = mLine.endPoint();
            return $"linet x{sp.x}y{sp.y}z{sp.z},x{ep.x}y{ep.y}z{ep.z}";
        }
    }
}
