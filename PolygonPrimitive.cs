﻿using CoreLib;

namespace Mini3DCad
{

    /// <summary>
    /// ポリゴンプリミティブ
    /// </summary>
    public class PolygonPrimitive : Primitive
    {
        public Polygon3D mPolygon;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PolygonPrimitive()
        {
            mPolygon = new Polygon3D();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="polygon">3Dポリゴン</param>
        /// <param name="color">色</param>
        /// <param name="face">2D平面</param>
        public PolygonPrimitive(Polygon3D polygon, FACE3D face = FACE3D.XY)
        {
            mPrimitiveId = PrimitiveId.Polygon;
            mPolygon = new Polygon3D(polygon);
            mPolygon.squeeze();
            if (mPolygon.isClockwise(face))
                mPolygon.reverse();
        }

        /// <summary>
        /// 3D座標リストの作成(三角形の集合)
        /// </summary>
        public override void createSurfaceData()
        {
            mSurfaceDataList = new List<SurfaceData>();
            SurfaceData surfaceData = new SurfaceData();
            if (mWireFrame) {
                surfaceData.mVertexList = mPolygon.toPoint3D(mDivideAngle);
                surfaceData.mVertexList.Add(surfaceData.mVertexList[0]);
                surfaceData.mDrawType = DRAWTYPE.LINE_STRIP;
                surfaceData.mFaceColor = mFaceColors[0];
            } else {
                (List<Point3D> triangles, bool reverse) = mPolygon.cnvTriangles(mDivideAngle);
                if (triangles.Count < 3)
                    return;
                surfaceData.mVertexList = triangles;
                surfaceData.mDrawType = DRAWTYPE.TRIANGLES;
                surfaceData.mFaceColor = mFaceColors[0];
                surfaceData.reverse(mReverse);
            }
            mSurfaceDataList.Add(surfaceData);
        }

        /// <summary>
        /// 2D表示用座標リストの作成
        /// </summary>
        public override void createVertexData()
        {
            mVertexList = new List<List<Point3D>>();
            if (!mSurfaceVertex) {
                mVertexList.Add(mPolygon.toPoint3D(mDivideAngle, true));
            } else {
                //  Debug(Surface表示確認)
                mVertexList.AddRange(getVertexList(mPolygon));
            }
        }

        /// <summary>
        /// ポリゴンから３角形の座標リストを作成
        /// </summary>
        /// <param name="polygon">ポリゴン</param>
        /// <returns>３角形座標リスト</returns>
        private List<List<Point3D>> getVertexList(Polygon3D polygon)
        {
            List<List<Point3D>> vertexList = new List<List<Point3D>>();
            (List<Point3D> triangles, bool reverse) = polygon.cnvTriangles(mDivideAngle);
            if (3 <= triangles.Count) {
                for (int i = 0; i < triangles.Count; i += 3) {
                    List<Point3D> plist = new List<Point3D> { triangles[i], triangles[i + 1], triangles[i + 2], triangles[i] };
                    vertexList.Add(plist);
                }
            }
            return vertexList;
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public override void translate(Point3D v, PointD pickPos, FACE3D face)
        {
            mPolygon.translate(v);
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">表示面</param>
        public override void rotate(Point3D cp, double ang, PointD pickPos, FACE3D face)
        {
            mPolygon.rotate(cp, ang, face);
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public override void offset(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            mPolygon.offset(sp, ep);
        }

        /// <summary>
        /// 座標を反転する
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        /// <param name="outline">3Dデータと外形線の作成</param>
        public override void mirror(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            mPolygon.mirror(new Line3D(sp, ep), face);
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
            mPolygon.scale(cp, scale);
        }

        /// <summary>
        /// ストレッチ
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">2D平面</param>
        public override void stretch(Point3D vec, bool arc, PointD pickPos, FACE3D face)
        {
            mPolygon.stretch(vec, new Point3D(pickPos, face), arc);
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
            PolygonD polygon = mPolygon.toPolygonD(face);
            if (primutive.mPrimitiveId == PrimitiveId.Point) {
                PointD p = ((PointPrimitive)primutive).mPoint.toPoint(face);
                List<PointD> iplist = polygon.intersection(p);
                if (iplist != null && 0 < iplist.Count)
                    ip = iplist.MinBy(p => p.length(pos));
            } else if (primutive.mPrimitiveId == PrimitiveId.Line) {
                LineD l = ((LinePrimitive)primutive).mLine.toLineD(face);
                List<PointD> iplist = polygon.intersection(l);
                if (iplist != null && 0 < iplist.Count)
                    ip = iplist.MinBy(p => p.length(pos));
            } else if (primutive.mPrimitiveId == PrimitiveId.Arc) {
                EllipseD eli = ((ArcPrimitive)primutive).mArc.toEllipseD(face);
                List<PointD> iplist = eli.intersection(polygon);
                if (iplist != null && 0 < iplist.Count)
                    ip = iplist.MinBy(p => p.length(pos));
            } else if (primutive.mPrimitiveId == PrimitiveId.Polyline) {
                PolylineD polyline = ((PolylinePrimitive)primutive).mPolyline.toPolylineD(face);
                List<PointD> iplist = polygon.intersection(polyline);
                if (iplist != null && 0 < iplist.Count)
                    ip = iplist.MinBy(p => p.length(pos));
            } else if (primutive.mPrimitiveId == PrimitiveId.Polygon) {
                PolygonD poly = ((PolygonPrimitive)primutive).mPolygon.toPolygonD(face);
                List<PointD> iplist = polygon.intersection(poly);
                if (iplist != null && 0 < iplist.Count)
                    ip = iplist.MinBy(p => p.length(pos));
            }
            if (ip != null)
                return mPolygon.intersection(ip, face);
            else
                return null;
        }

        /// <summary>
        /// 固有データを文字列配列に変換
        /// </summary>
        /// <returns>文字列配列</returns>
        public override string[] toDataList()
        {
            bool multi = mPolygon.IsMultiType();
            List<string> dataList = new List<string>() {
                "PolygonData",
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
            if (0 == list.Length || list[0] != "PolygonData")
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
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine($"Polygon setDataList {e.ToString()}");
            }
        }

        /// <summary>
        /// 固有データ情報
        /// </summary>
        /// <param name="form">書式</param>
        /// <returns>文字列</returns>
        public override string dataInfo(string form)
        {
            string buf = "PolygonData:";
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
            List<Point3D> plist = mPolygon.toPoint3D();
            string buf = "";
            for (int i = 0; i < plist.Count && i < 4; i++)
                buf += plist[i].ToString(form) + ",";
            buf.TrimEnd(',');
            return $"長さ:{mPolygon.length().ToString("F3")},数:{mPolygon.mPolygon.Count},座標:{buf}";
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        public override Primitive toCopy()
        {
            PolygonPrimitive polygon = new PolygonPrimitive();
            polygon.copyProperty(this, true, true);
            polygon.mPolygon = mPolygon.toCopy();
            return polygon;
        }

        /// <summary>
        /// 座標点リストを求める
        /// </summary>
        /// <returns>ポリライン</returns>
        public override Polyline3D toPointList()
        {
            return mPolygon.toPolyline3D();
        }

        /// <summary>
        /// 座標点リストを求める
        /// </summary>
        /// <returns>ポリライン</returns>
        public override Polyline3D getVertexList()
        {
            return mPolygon.toPolyline3D();
        }

        /// <summary>
        /// コマンド出力
        /// </summary>
        /// <returns></returns>
        public override string toCommand()
        {
            string buf = $"cx{mPolygon.mCp.x}cy{mPolygon.mCp.y}cz{mPolygon.mCp.z}";
            buf += $",ux{mPolygon.mU.x}uy{mPolygon.mU.y}uz{mPolygon.mU.z}";
            buf += $",vx{mPolygon.mV.x}vy{mPolygon.mV.y}vz{mPolygon.mV.z}";
            for (int i = 0; i < mPolygon.mPolygon.Count; i++)
                buf += $",x{mPolygon.mPolygon[i].x}y{mPolygon.mPolygon[i].y}";
            return $"polygon ";
        }
    }
}
