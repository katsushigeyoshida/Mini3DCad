using CoreLib;

namespace Mini3DCad
{

    /// <summary>
    /// 点プリミティブ
    /// </summary>
    public class PointPrimitive : Primitive
    {
        public Point3D mPoint;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PointPrimitive()
        {
            mPrimitiveId = PrimitiveId.Point;
            mPoint = new Point3D();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="p">3D点座標</param>
        /// <param name="face">作成面</param>
        public PointPrimitive(Point3D p)
        {
            mPrimitiveId = PrimitiveId.Point;
            mPoint = p.toCopy();
        }

        /// <summary>
        /// 3D座標(Surface)リストの作成
        /// </summary>
        /// <returns>座標リスト</returns>
        public override void createSurfaceData()
        {
            mSurfaceDataList = new List<SurfaceData>();
            SurfaceData surfaceData = new SurfaceData();
            surfaceData.mVertexList = [mPoint.toCopy()];
            surfaceData.mDrawType = DRAWTYPE.POINTS;
            surfaceData.mFaceColor = mLineColor;
            mSurfaceDataList.Add(surfaceData);
        }

        /// <summary>
        /// 2D表示用座標リストの作成
        /// </summary>
        public override void createVertexData()
        {
            mVertexList = [[mPoint]];
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public override void translate(Point3D v, PointD pickPos, FACE3D face)
        {
            mPoint.translate(v);
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">2D平面</param>
        public override void rotate(Point3D cp, double ang, PointD pickPos, FACE3D face)
        {
            mPoint.rotate(cp, ang, face);
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public override void offset(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            Point3D v = ep - sp;
            mPoint.offset(v);
        }

        /// <summary>
        /// 点の座標を反転する
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public override void mirror(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            Line3D l = new Line3D(sp, ep);
            mPoint = l.mirror(mPoint, face);
        }

        /// <summary>
        /// トリム
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public override void trim(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {

        }

        /// <summary>
        /// 拡大縮小
        /// </summary>
        /// <param name="cp">拡大中心</param>
        /// <param name="scale">倍率</param>
        /// <param name="face">2D平面</param>
        public override void scale(Point3D cp, double scale, PointD pickPos, FACE3D face)
        {
            mPoint.scale(cp, scale);
        }

        /// <summary>
        /// ストレッチ
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="pickPos"></param>
        /// <param name="face"></param>
        public override void stretch(Point3D vec, bool arc, PointD pickPos, FACE3D face)
        {
            mPoint.translate(vec);
        }


        /// <summary>
        /// 固有データを文字列配列に変換
        /// </summary>
        /// <returns>文字列配列</returns>
        public override string[] toDataList()
        {
            List<string> dataList = new List<string>() {
                "PointData",
                "Point", mPoint.x.ToString(), mPoint.y.ToString(), mPoint.z.ToString(),
            };
            return dataList.ToArray();
        }

        /// <summary>
        /// 文字列配列から固有データを設定
        /// </summary>
        /// <param name="list">文字列配列</param>
        public override void setDataList(string[] list)
        {
            if (0 == list.Length || list[0] != "PointData")
                return;
            try {
                double val;
                for (int i = 1; i < list.Length; i++) {
                    if (list[i] == "Point") {
                        mPoint.x = double.TryParse(list[++i], out val) ? val : 0;
                        mPoint.y = double.TryParse(list[++i], out val) ? val : 0;
                        mPoint.z = double.TryParse(list[++i], out val) ? val : 0;
                    }
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine($"Point setDataList {e.ToString()}");
            }
        }

        /// <summary>
        /// 固有データ情報
        /// </summary>
        /// <param name="form">書式</param>
        /// <returns>文字列</returns>
        public override string dataInfo(string form)
        {
            return "PointData:" +
                " Point " + mPoint.ToString(form);
        }

        /// <summary>
        /// サマリデータ
        /// </summary>
        /// <param name="form">データ書式</param>
        /// <returns>文字列</returns>
        public override string dataSummary(string form = "F2")
        {
            return $"座標:{mPoint.ToString(form)}";
        }


        /// <summary>
        /// コピーを作成
        /// </summary>
        public override Primitive toCopy()
        {
            PointPrimitive point = new PointPrimitive();
            point.copyProperty(this, true, true);
            point.mPoint = mPoint.toCopy();
            return point;
        }

        /// <summary>
        /// 座標点リストの取得
        /// </summary>
        /// <returns>ポリライン</returns>
        public override Polyline3D toPointList()
        {
            return new Polyline3D([mPoint]);
        }

        /// <summary>
        /// 座標点リストを求める
        /// </summary>
        /// <returns>ポリライン</returns>
        public override Polyline3D getVertexList()
        {
            return new Polyline3D([mPoint]);
        }

        /// <summary>
        /// コマンド出力
        /// </summary>
        /// <returns></returns>
        public override string toCommand()
        {
            return $"point x{mPoint.x}y{mPoint.y}z{mPoint.z}";
        }
    }
}
