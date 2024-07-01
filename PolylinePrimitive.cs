using CoreLib;
using Brush = System.Windows.Media.Brush;

namespace Mini3DCad
{

    /// <summary>
    /// ポリラインプリミティブ
    /// </summary>
    public class PolylinePrimitive : Primitive
    {
        public Polyline3D mPolyline;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PolylinePrimitive()
        {
            mPolyline = new Polyline3D();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="polyline">ポリライン</param>
        /// <param name="color">カラー</param>
        /// <param name="face">2D平面</param>
        public PolylinePrimitive(Polyline3D polyline)
        {
            mPrimitiveId = PrimitiveId.Polyline;
            mPolyline = polyline.toCopy();
            mPolyline.squeeze();
        }

        /// <summary>
        /// 3D座標リストの作成
        /// </summary>
        public override void createSurfaceData()
        {
            mSurfaceDataList = new List<SurfaceData>();
            SurfaceData surfaceData = new SurfaceData();
            surfaceData.mVertexList = mPolyline.toPoint3D(mDivideAngle);
            surfaceData.mDrawType = DRAWTYPE.LINE_STRIP;
            surfaceData.mFaceColor = mLineColor;
            mSurfaceDataList.Add(surfaceData);
        }

        /// <summary>
        /// 2D表示用座標リストの作成
        /// </summary>
        public override void createVertexData()
        {
            mVertexList = new List<List<Point3D>>() {
                mPolyline.toPoint3D(mDivideAngle / 2)
            };
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public override void translate(Point3D v, PointD pickPos, FACE3D face)
        {
            mPolyline.translate(v);
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">表示面</param>
        public override void rotate(Point3D cp, double ang, PointD pickPos, FACE3D face)
        {
            mPolyline.rotate(cp, ang, face);
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public override void offset(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            mPolyline.offset(sp, ep);
        }

        /// <summary>
        /// 座標を反転する
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public override void mirror(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            mPolyline.mirror(new Line3D(sp, ep), face);
        }

        /// <summary>
        /// トリム
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public override void trim(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            mPolyline.trim(sp, ep);
        }

        /// <summary>
        /// 拡大縮小
        /// </summary>
        /// <param name="cp">拡大中心</param>
        /// <param name="scale">倍率</param>
        /// <param name="face">2D平面</param>
        public override void scale(Point3D cp, double scale, PointD pickPos, FACE3D face)
        {
            mPolyline.scale(cp, scale);
        }

        /// <summary>
        /// ストレッチ
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">2D平面</param>
        public override void stretch(Point3D vec, bool arc, PointD pickPos, FACE3D face)
        {
            mPolyline.stretch(vec, new Point3D(pickPos, face), arc);
        }

        /// <summary>
        /// 固有データを文字列配列に変換
        /// </summary>
        /// <returns>文字列配列</returns>
        public override string[] toDataList()
        {
            bool multi = mPolyline.IsMultiType();
            List<string> dataList = new List<string>() {
                "PolylineData",
                "Cp", mPolyline.mCp.x.ToString(), mPolyline.mCp.y.ToString(), mPolyline.mCp.z.ToString(),
                "U", mPolyline.mU.x.ToString(), mPolyline.mU.y.ToString(), mPolyline.mU.z.ToString(),
                "V", mPolyline.mV.x.ToString(), mPolyline.mV.y.ToString(), mPolyline.mV.z.ToString(),
                "Size", mPolyline.mPolyline.Count.ToString(),
                "Multi", multi.ToString(),
            };
            for (int i = 0; i < mPolyline.mPolyline.Count; i++) {
                dataList.Add(mPolyline.mPolyline[i].x.ToString());
                dataList.Add(mPolyline.mPolyline[i].y.ToString());
                if (multi)
                    dataList.Add(mPolyline.mPolyline[i].type.ToString());
            }
            return dataList.ToArray();
        }

        /// <summary>
        /// 文字列配列から固有データを設定
        /// </summary>
        /// <param name="list">文字列配列</param>
        public override void setDataList(string[] list)
        {
            if (0 == list.Length || list[0] != "PolylineData")
                return;
            try {
                int ival;
                double val;
                int count;
                bool multi = false;
                bool bval;
                for (int i = 1; i < list.Length; i++) {
                    if (list[i] == "Cp") {
                        Point3D p = new Point3D();
                        p.x = double.TryParse(list[++i], out val) ? val : 0;
                        p.y = double.TryParse(list[++i], out val) ? val : 0;
                        p.z = double.TryParse(list[++i], out val) ? val : 0;
                        mPolyline.mCp = p;
                    } else if (list[i] == "U") {
                        Point3D p = new Point3D();
                        p.x = double.TryParse(list[++i], out val) ? val : 0;
                        p.y = double.TryParse(list[++i], out val) ? val : 0;
                        p.z = double.TryParse(list[++i], out val) ? val : 0;
                        mPolyline.mU = p;
                    } else if (list[i] == "V") {
                        Point3D p = new Point3D();
                        p.x = double.TryParse(list[++i], out val) ? val : 0;
                        p.y = double.TryParse(list[++i], out val) ? val : 0;
                        p.z = double.TryParse(list[++i], out val) ? val : 0;
                        mPolyline.mV = p;
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
                        mPolyline.mPolyline.Add(p);
                    }
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine($"Polyline setDataList {e.ToString()}");
            }
        }

        /// <summary>
        /// 固有データ情報
        /// </summary>
        /// <param name="form">書式</param>
        /// <returns>文字列</returns>
        public override string dataInfo(string form)
        {
            string buf = "PolylineData:";
            buf += $"Cp {mPolyline.mCp.x.ToString(form)},{mPolyline.mCp.y.ToString(form)},{mPolyline.mCp.z.ToString(form)},";
            buf += $"U {mPolyline.mU.x.ToString(form)},{mPolyline.mU.y.ToString(form)},{mPolyline.mU.z.ToString(form)},";
            buf += $"V {mPolyline.mV.x.ToString(form)},{mPolyline.mV.y.ToString(form)},{mPolyline.mV.z.ToString(form)},";
            buf += $"Size {mPolyline.mPolyline.Count} : ";
            for (int i = 0; i < mPolyline.mPolyline.Count; i++) {
                buf += "," + mPolyline.mPolyline[i].ToString(form);
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
            List<Point3D> plist = mPolyline.toPoint3D();
            string buf = "";
            for (int i = 0; i < plist.Count && i < 5; i++)
                buf += plist[i].ToString(form) + ",";
            buf = buf.TrimEnd(',');
            if (5 < plist.Count)
                buf += ",・・・";
            return $"長さ:{mPolyline.length().ToString("F3")},数:{mPolyline.mPolyline.Count},座標:{buf}";
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        public override Primitive toCopy()
        {
            PolylinePrimitive polyline = new PolylinePrimitive();
            polyline.copyProperty(this, true, true);
            polyline.mPolyline = mPolyline.toCopy();
            return polyline;
        }

        /// <summary>
        /// 座標点リストを求める
        /// </summary>
        /// <returns>ポリライン</returns>
        public override Polyline3D toPointList()
        {
            return mPolyline.toCopy();
        }

        /// <summary>
        /// 座標点リストを求める
        /// </summary>
        /// <returns>ポリライン</returns>
        public override Polyline3D getVertexList()
        {
            return mPolyline.toCopy();
        }

        /// <summary>
        /// コマンド出力
        /// </summary>
        /// <returns></returns>
        public override string toCommand()
        {
            string buf = $"cx{mPolyline.mCp.x}cy{mPolyline.mCp.y}cz{mPolyline.mCp.z}";
            buf += $",ux{mPolyline.mU.x}uy{mPolyline.mU.y}uz{mPolyline.mU.z}";
            buf += $",vx{mPolyline.mV.x}vy{mPolyline.mV.y}vz{mPolyline.mV.z}";
            for (int i = 0; i < mPolyline.mPolyline.Count; i++)
                buf += $",x{mPolyline.mPolyline[i].x}y{mPolyline.mPolyline[i].y}";
            return $"polyline {buf}";
        }
    }
}
