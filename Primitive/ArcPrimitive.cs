using CoreLib;

namespace Mini3DCad
{

    /// <summary>
    /// 円/円弧プリミティブ
    /// </summary>
    public class ArcPrimitive : Primitive
    {
        public Arc3D mArc;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ArcPrimitive()
        {
            mPrimitiveId = PrimitiveId.Arc;
            mArc = new Arc3D();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="arc">3D円弧</param>
        /// <param name="color">カラー</param>
        /// <param name="face">2D平面</param>
        /// <param name="divideAng">分割角度</param>
        public ArcPrimitive(Arc3D arc,  double divideAng = Math.PI / 15)
        {
            mPrimitiveId = PrimitiveId.Arc;
            mArc = arc.toCopy();
            mDivideAngle = divideAng;
        }

        /// <summary>
        /// 3D座標(Surface)リストの作成
        /// </summary>
        /// <returns>座標リスト</returns>
        public override void createSurfaceData()
        {
            mSurfaceDataList = new List<SurfaceData>();
            SurfaceData surfaceData = new SurfaceData();
            surfaceData.mVertexList = mArc.toPoint3D(mDivideAngle);
            surfaceData.mDrawType = DRAWTYPE.LINE_STRIP;
            surfaceData.mFaceColor = mLineColor;
            mSurfaceDataList.Add(surfaceData);
        }

        /// <summary>
        /// 2D表示用座標データの作成
        /// </summary>
        public override void createVertexData()
        {
            mVertexList = new List<List<Point3D>>();
            mVertexList.Add(mArc.toPoint3D());
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public override void translate(Point3D v, PointD pickPos, FACE3D face)
        {
            mArc.translate(v);
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">表示面</param>
        public override void rotate(Point3D cp, double ang, PointD pickPos, FACE3D face)
        {
            mArc.rotate(cp, ang, face);
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public override void offset(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            mArc.offset(sp, ep);
        }

        /// <summary>
        /// 円弧の座標を反転する
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        /// <param name="outline">3Dデータと外形線の作成</param>
        public override void mirror(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            Line3D l = new Line3D(sp, ep);
            mArc.mirror(l, face);
        }

        /// <summary>
        /// トリム
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public override void trim(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            mArc.trim(sp, ep);
        }

        /// <summary>
        /// 拡大縮小
        /// </summary>
        /// <param name="cp">拡大中心</param>
        /// <param name="scale">倍率</param>
        /// <param name="face">2D平面</param>
        public override void scale(Point3D cp, double scale, PointD pickPos, FACE3D face)
        {
            mArc.mCp.scale(cp, scale);
            mArc.mR *= scale;
        }

        /// <summary>
        /// ストレッチ
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">2D平面</param>
        public override void stretch(Point3D vec, bool arc, PointD pickPos, FACE3D face)
        {
            mArc.stretch(vec, new Point3D(pickPos, face));
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
            if (primutive.mPrimitiveId == PrimitiveId.Point) {
                Point3D point = ((PointPrimitive)primutive).mPoint;
                return mArc.intersection(point, face);
            } else if (primutive.mPrimitiveId == PrimitiveId.Line) {
                Line3D line = ((LinePrimitive)primutive).mLine;
                return mArc.intersection(line, pos, face);
            } else if (primutive.mPrimitiveId == PrimitiveId.Arc) {
                Arc3D arc = ((ArcPrimitive)primutive).mArc;
                return mArc.intersection(arc, pos, face);
            } else if (primutive.mPrimitiveId == PrimitiveId.Polyline) {
                Polyline3D polyline = ((PolylinePrimitive)primutive).mPolyline;
                return polyline.intersection(mArc, pos, face);
            } else if (primutive.mPrimitiveId == PrimitiveId.Polygon) {
                Polygon3D polygon = ((PolygonPrimitive)primutive).mPolygon;
                return polygon.intersection(mArc, pos, face);
            }
            return null;
        }

        /// <summary>
        /// 固有データを文字列配列に変換
        /// </summary>
        /// <returns>文字列配列リスト</returns>
        public override List<string[]> toDataList()
        {
            List<string> dataList = new List<string>() {
                "ArcData",
                "Cp", mArc.mCp.x.ToString(), mArc.mCp.y.ToString(), mArc.mCp.z.ToString(),
                "R", mArc.mR.ToString(),
                "U", mArc.mU.x.ToString(), mArc.mU.y.ToString(), mArc.mU.z.ToString(),
                "V", mArc.mV.x.ToString(), mArc.mV.y.ToString(), mArc.mV.z.ToString(),
                "Sa", mArc.mSa.ToString(), "Ea", mArc.mEa.ToString()
            };
            return new List<string[]>() { dataList.ToArray() };
        }

        /// <summary>
        /// 文字列配列から固有データを設定
        /// </summary>
        /// <param name="dataList">文字列配列リスト</param>
        /// <param name="sp">文字列配列位置</param>
        /// <returns>文字列配列位置</returns>
        public override int setDataList(List<string[]> dataList, int sp)
        {
            string[] list = dataList[sp++];
            if (0 == list.Length || list[0] != "ArcData")
                return sp;
            try {
                double val;
                for (int i = 1; i < list.Length; i++) {
                    if (list[i] == "Cp") {
                        mArc.mCp.x = double.TryParse(list[++i], out val) ? val : 0;
                        mArc.mCp.y = double.TryParse(list[++i], out val) ? val : 0;
                        mArc.mCp.z = double.TryParse(list[++i], out val) ? val : 0;
                    } else if (list[i] == "R") {
                        mArc.mR = double.TryParse(list[++i], out val) ? val : 0;
                    } else if (list[i] == "U") {
                        mArc.mU.x = double.TryParse(list[++i], out val) ? val : 0;
                        mArc.mU.y = double.TryParse(list[++i], out val) ? val : 0;
                        mArc.mU.z = double.TryParse(list[++i], out val) ? val : 0;
                    } else if (list[i] == "V") {
                        mArc.mV.x = double.TryParse(list[++i], out val) ? val : 0;
                        mArc.mV.y = double.TryParse(list[++i], out val) ? val : 0;
                        mArc.mV.z = double.TryParse(list[++i], out val) ? val : 0;
                    } else if (list[i] == "Sa") {
                        mArc.mSa = double.TryParse(list[++i], out val) ? val : 0;
                    } else if (list[i] == "Ea") {
                        mArc.mEa = double.TryParse(list[++i], out val) ? val : 0;
                    }
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine($"Arc setDataList {e.ToString()}");
            }
            return sp;
        }

        /// <summary>
        /// 固有データ情報
        /// </summary>
        /// <param name="form">書式</param>
        /// <returns>文字列</returns>
        public override string dataInfo(string form)
        {
            return "ArcData:" +
                " Cp " + mArc.mCp.ToString(form) +
                " R " + mArc.mR.ToString(form) +
                " U " + mArc.mU.ToString(form) +
                " V " + mArc.mV.ToString(form) +
                " Sa " + mArc.mSa.ToString(form) + " Ea " + mArc.mEa.ToString(form);
        }

        /// <summary>
        /// サマリデータ
        /// </summary>
        /// <param name="form">データ書式</param>
        /// <returns>文字列</returns>
        public override string dataSummary(string form = "F2")
        {
            return $"中心:{mArc.mCp.ToString(form)},半径:{mArc.mR.ToString(form)},始終角:{ylib.R2D(mArc.mSa).ToString(form)},{ylib.R2D(mArc.mEa).ToString(form)}";
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        public override Primitive toCopy()
        {
            ArcPrimitive arc = new ArcPrimitive();
            arc.copyProperty(this, true, true);
            arc.mArc = mArc.toCopy();
            arc.mDivideAngle = mDivideAngle;
            return arc;
        }

        /// <summary>
        /// 座標点リストを求める
        /// </summary>
        /// <returns>ポリライン</returns>
        public override Polyline3D toPointList()
        {
            int divNo = Math.PI * 1.9 < (mArc.mEa - mArc.mSa) ? 4 : 2;
            List<Point3D> plist = mArc.toPoint3D(divNo);
            Polyline3D polyline = new Polyline3D(plist);
            polyline.mPolyline[1].type = 1;
            if (divNo == 4)
                polyline.mPolyline[3].type = 1;
            return polyline;
        }

        /// <summary>
        /// 座標点リストを求める
        /// </summary>
        /// <returns>ポリライン</returns>
        public override Polyline3D getVertexList()
        {
            return new Polyline3D(mArc.toPoint3D(mDivideAngle));
        }

        /// <summary>
        /// コマンド出力
        /// </summary>
        /// <returns></returns>
        public override string toCommand()
        {
            string buf = $"cx{mArc.mCp.x}cy{mArc.mCp.y}cz{mArc.mCp.z}";
            buf += $",ux{mArc.mU.x}uy{mArc.mU.y}uz{mArc.mU.z}";
            buf += $",vx{mArc.mV.x}vy{mArc.mV.y}vz{mArc.mV.z}";
            buf += $",r{mArc.mR}";
            buf += $",sa{mArc.mSa}ea{mArc.mEa}";
            return $"arc {buf}";
        }
    }
}
