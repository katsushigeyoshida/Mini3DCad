using CoreLib;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace Mini3DCad
{
    /// <summary>
    /// プリミティブの種類
    /// </summary>
    public enum PrimitiveId
    {
        Non, Link,
        Point, Line, Arc, Polyline, Polygon,
        WireCube, Cube, Cylinder, Sphere, Cone,
        Extrusion, Revolution, Sweep
    }

    /// <summary>
    /// 描画方式
    /// </summary>
    public enum DRAWTYPE
    {
        POINTS, LINES, LINE_STRIP, LINE_LOOP,
        TRIANGLES, QUADS, POLYGON, TRIANGLE_STRIP,
        QUAD_STRIP, TRIANGLE_FAN, MULTI
    };

    /// <summary>
    /// Surfaceの元データ
    /// </summary>
    public class SurfaceData
    {
        public List<Point3D> mVertexList;           //  座標点リスト
        public DRAWTYPE mDrawType;                  //  描画方式
        public Brush mFaceColor = Brushes.Blue;

        /// <summary>
        /// 座標点の移動
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public void translate(Point3D v)
        {
            for (int i = 0; i <  mVertexList.Count; i++) {
                mVertexList[i].translate(v);
            }
        }

        /// <summary>
        /// 座標点の回転
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">回転面</param>
        public void rotate(Point3D cp, double ang, FACE3D face)
        {
            for (int i = 0; i < mVertexList.Count; i++) {
                mVertexList[i].rotate(cp, ang, face);
            }
        }

        /// <summary>
        /// 面データを逆回りに変換する
        /// </summary>
        /// <param name="reverse">変換する</param>
        public void reverse(bool reverse = true)
        {
            if (!reverse)
                return;
            Point3D t = new Point3D();
            switch (mDrawType) {
                case DRAWTYPE.TRIANGLES:
                    for (int i = 0; i < mVertexList.Count; i += 3) {
                        t = mVertexList[i];
                        mVertexList[i] = mVertexList[i + 2];
                        mVertexList[i + 2] = t;
                    }
                    break;
                case DRAWTYPE.QUADS:
                    for (int i = 0; i < mVertexList.Count; i += 4) {
                        t = mVertexList[i];
                        mVertexList[i + 1] = mVertexList[i + 3];
                        mVertexList[i + 3] = t;
                    }
                    break;
                case DRAWTYPE.TRIANGLE_STRIP:
                case DRAWTYPE.QUAD_STRIP:
                    for (int i = 0; i < mVertexList.Count; i += 2) {
                        t = mVertexList[i];
                        mVertexList[i] = mVertexList[i + 1];
                        mVertexList[i + 1] = t;
                    }
                    break;
                case DRAWTYPE.TRIANGLE_FAN:
                    t = mVertexList[0];
                    mVertexList.RemoveAt(0);
                    mVertexList.Reverse();
                    mVertexList.Insert(0, t);
                    break;
                case DRAWTYPE.POLYGON:
                    mVertexList.Reverse();
                    break;
            }
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        /// <returns></returns>
        public SurfaceData toCopy()
        {
            SurfaceData surfaceData = new SurfaceData();
            surfaceData.mVertexList = mVertexList.ConvertAll(p => p.toCopy());
            surfaceData.mDrawType = mDrawType;
            surfaceData.mFaceColor = mFaceColor;
            return surfaceData;
        }

        /// <summary>
        /// 文字列に変換
        /// </summary>
        /// <returns></returns>
        public string toString(string form = "F2")
        {
            string buf = mDrawType.ToString();
            for (int i = 0; i < mVertexList.Count; i++)
                buf += "," + mVertexList[i].ToString(form);
            return buf;
        }

        /// <summary>
        /// 座標点リストを2Dに変換する
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public List<PointD> toPointDList(FACE3D face)
        {
            return mVertexList.ConvertAll(p => p.toPoint(face));
        }
    }

    /// <summary>
    /// プリミティブの
    /// </summary>
    public abstract class Primitive
    {
        public PrimitiveId mPrimitiveId = PrimitiveId.Non;                      //  種別
        public FACE3D mPrimitiveFace = FACE3D.XY;                               //  表示面(xy,yz,zx)
        public double mLineThickness = 1.0;                                     //  線の太さ
        public int mLineType = 0;                                               //  線種(0:solid 1:dash 2:center 3:phantom)
        public Brush mLineColor = Brushes.Black;                                //  線の色
        public List<Brush> mFaceColors = new List<Brush>() { Brushes.Blue };    //  面の色
        public Brush mPickColor = Brushes.Red;                                  //  ピック時のカラー
        public bool mPick = false;                                              //  ピック状態
        public List<SurfaceData> mSurfaceDataList;                              //  3D座標データ
        public List<List<Point3D>> mVertexList;                                 //  2D表示用3D座標データ
        public bool mReverse = false;                                           //  Surfaceの座標回転方向変換
        public bool mSurfaceVertex = false;                                     //  Debug用
        public double mDivideAngle = Math.PI / 15;

        public YLib ylib = new YLib();

        /// <summary>
        /// 3D座標(Surface)リストの作成
        /// </summary>
        public abstract void createSurfaceData();

        /// <summary>
        /// 2D表示用座標データの作成
        /// </summary>
        public abstract void createVertexData();

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public abstract void translate(Point3D v);

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">操作面</param>
        public abstract void rotate(Point3D cp, double ang, FACE3D face);

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">操作面</param>
        public abstract void offset(Point3D sp, Point3D ep, FACE3D face);

        /// <summary>
        /// ミラー
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D操作面</param>
        public abstract void mirror(Point3D sp, Point3D ep, FACE3D face);

        /// <summary>
        /// トリム
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public abstract void trim(Point3D sp, Point3D ep, FACE3D face);

        /// <summary>
        /// 拡大縮小
        /// </summary>
        /// <param name="cp">拡大中心</param>
        /// <param name="scale">倍率</param>
        /// <param name="face">2D平面</param>
        public abstract void scale(Point3D cp, double scale, FACE3D face);

        /// <summary>
        /// ストレッチ
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">"D平面</param>
        public abstract void stretch(Point3D vec, PointD pickPos, bool arc, FACE3D face);


        /// <summary>
        /// 固有データを文字列配列に変換
        /// </summary>
        /// <returns>文字列配列</returns>
        public abstract string[] toDataList();

        /// <summary>
        /// 文字列配列から固有データを設定
        /// </summary>
        /// <param name="list">文字列配列</param>
        public abstract void setDataList(string[] list);

        /// <summary>
        /// プリミティブの個別情報
        /// </summary>
        /// <param name="form">書式</param>
        /// <returns>文字列</returns>
        public abstract string dataInfo(string form);

        /// <summary>
        /// 簡易データ情報
        /// </summary>
        /// <param name="form">書式</param>
        /// <returns>文字列</returns>
        public abstract string dataSummary(string form);

        /// <summary>
        /// コピーを作成
        /// </summary>
        public abstract Primitive toCopy();

        /// <summary>
        /// 座標点の取得
        /// </summary>
        /// <returns></returns>
        public abstract Polyline3D toPointList();

        /// <summary>
        /// 座標点リストをPolyline3Dで取得
        /// </summary>
        /// <returns></returns>
        public abstract Polyline3D getVertexList();

        /// <summary>
        /// キーコマンドに変換
        /// </summary>
        /// <returns></returns>
        public abstract string toCommand();

        /// <summary>
        /// VertexListデータの移動
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public void translateVertexList(Point3D v)
        {
            for (int i = 0; i < mVertexList.Count; i++) {
                for (int j = 0; j < mVertexList[i].Count; j++)
                    mVertexList[i][j].translate(v);
            }
        }

        /// <summary>
        /// VertesListデータの回転
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角度</param>
        /// <param name="face">2D平面</param>
        public void rotateVertexList(Point3D cp, double ang, FACE3D face)
        {
            for (int i = 0; i < mVertexList.Count; i++) {
                for (int j = 0; j < mVertexList[i].Count; j++)
                    mVertexList[i][j].rotate(cp, ang, face);
            }
        }

        /// <summary>
        /// 属性をコピーする
        /// </summary>
        /// <param name="primitive">Primitive</param>
        /// <param name="dataList">データリスト</param>
        /// <param name="id">Primitive ID</param>
        public void copyProperty(Primitive primitive, bool dataList = false, bool id = false)
        {
            if (id)
                mPrimitiveId = primitive.mPrimitiveId;
            mPrimitiveFace = primitive.mPrimitiveFace;
            mLineThickness = primitive.mLineThickness;
            mLineColor = primitive.mLineColor;
            mLineType = primitive.mLineType;
            mFaceColors[0] = primitive.mFaceColors[0];
            mPickColor = primitive.mPickColor;
            mPick = primitive.mPick;
            if (dataList) {
                //copySurfaceDataList(primitive);
                copyVertexList(primitive);
            }
            mReverse = primitive.mReverse;
            mDivideAngle = primitive.mDivideAngle;
            mSurfaceVertex = primitive.mSurfaceVertex;
        }

        /// <summary>
        /// 3D座標データ(SurfaceData)のコピー
        /// </summary>
        /// <param name="primitive">プリミティブ</param>
        public void copySurfaceDataList(Primitive primitive)
        {
            mSurfaceDataList = primitive.mSurfaceDataList.ConvertAll(p => p.toCopy());
        }

        /// <summary>
        /// 2D表示用座標データのコピー
        /// </summary>
        /// <param name="primitive">プリミティブ</param>
        public void copyVertexList(Primitive primitive)
        {
            mVertexList = new List<List<Point3D>>();
            for (int i = 0; i < primitive.mVertexList.Count; i++) {
                List<Point3D> plist = primitive.mVertexList[i].ConvertAll(p => p.toCopy());
                mVertexList.Add(plist);
            }
        }

        /// <summary>
        /// 2D 表示 外形線の表示
        /// </summary>
        /// <param name="draw"></param>
        /// <param name="face">表示 2D平面</param>
        public void draw2D(YWorldDraw draw, FACE3D face)
        {
            if (mPick)
                draw.mBrush = mPickColor;
            else
                draw.mBrush = mLineColor;
            draw.mThickness = mLineThickness;
            draw.mLineType = mLineType;
            draw.mPointSize = 3;
            draw.mPointType = 0;
            for (int i = 0; i < mVertexList.Count; i++) {
                List<PointD> plist = mVertexList[i].ConvertAll(p => p.toPoint(face));
                if (1 < plist.Count)
                    draw.drawWPolyline(plist);
                else if (plist.Count == 1)
                    draw.drawWPoint(plist[0]);
            }
        }


        /// <summary>
        /// VertexListを利用して要素のピック有無を判定
        /// </summary>
        /// <param name="b">ピックボックス</param>
        /// <param name="face">表示2D平面</param>
        /// <returns></returns>
        public bool pickChk(Box b, FACE3D face)
        {
            for (int i = 0; i < mVertexList.Count; i++) {
                List<PointD> plist = mVertexList[i].ConvertAll(p => p.toPoint(face));
                if (0 < b.intersection(plist, false, true).Count || b.insideChk(plist))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 2D平面上で最も近い分割点の座標を求める
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="divideNo"></param>
        /// <param name="face"></param>
        /// <returns></returns>
        public PointD nearPoint(PointD pos, int divideNo, FACE3D face)
        {
            List<LineD> llist = new List<LineD>();
            for (int i = 0; i < mVertexList.Count; i++) {
                for (int j = 0; j < mVertexList[i].Count - 1; j++) {
                    LineD line = new LineD(mVertexList[i][j].toPoint(face), mVertexList[i][j + 1].toPoint(face));
                    llist.Add(line);
                }
            }
            int n = -1;
            double dis = double.MaxValue;
            for (int i = 0; i < llist.Count; i++) {
                PointD ip = llist[i].intersection(pos);
                double l = ip.length(pos);
                if (dis > l && llist[i].onPoint(ip)) {
                    dis = l;
                    n = i;
                }
            }
            if (n < 0)
                return null;
            if (divideNo <= 0)
                return llist[n].intersection(pos);
            List<PointD> plist = llist[n].dividePoints(divideNo);
            return plist.MinBy(p => p.length(pos));
        }

        /// <summary>
        /// 線分の取得
        /// </summary>
        /// <param name="pos">指定点</param>
        /// <param name="face">2D平面</param>
        /// <returns>3D線分</returns>
        public Line3D getLine(PointD pos, FACE3D face)
        {
            List<Line3D> llist = new List<Line3D>();
            for (int i = 0; i < mVertexList.Count; i++) {
                for (int j = 0; j < mVertexList[i].Count - 1; j++) {
                    Line3D line = new Line3D(mVertexList[i][j], mVertexList[i][j + 1]);
                    llist.Add(line);
                }
            }
            int n = -1;
            double dis = double.MaxValue;
            for (int i = 0; i < llist.Count; i++) {
                PointD ip = llist[i].intersection(pos, face).toPoint(face);
                double l = ip.length(pos);
                if (dis > l) {
                    dis = l;
                    n = i;
                }
            }
            return 0 <= n ? llist[n].toCopy() : null;
        }

        /// <summary>
        /// 属性情報
        /// </summary>
        /// <returns></returns>
        public string propertyInfo()
        {
            string buf = $"ID: {mPrimitiveId}, Face: {mPrimitiveFace},";
            buf += $" LineColor: {ylib.getBrushName(mLineColor)}, FaceColor: {ylib.getBrushName(mFaceColors[0])},";
            buf += $"\nLineType: {mLineType}, Thickness: {mLineThickness}, Reverse: {mReverse}, DivideAngle: {mDivideAngle}";
            buf += $" Area: {getArea().ToString("F2")}";
            return buf;
        }

        /// <summary>
        /// 3D座標情報(VertexList)
        /// </summary>
        /// <returns></returns>
        public List<string> vertexInfo(string form = "F2")
        {
            List<string> vertexList = new List<string>();
            for (int i = 0; i < mVertexList.Count; i++) {
                for (int j = 0; j < mVertexList[i].Count; j++)
                    vertexList.Add(mVertexList[i][j].ToString(form));
            }
            return vertexList;
        }

        /// <summary>
        /// 3D座標情報(SurfaceData)
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public List<string> surfaceInfo(string form = "F2")
        {
            List<string> surfaceList = new List<string>();
            for (int i = 0; i < mSurfaceDataList.Count; i++) {
                surfaceList.Add(mSurfaceDataList[i].toString(form));
            }
            return surfaceList;
        }

        /// <summary>
        /// プロパティデータを文字列リストに変換
        /// </summary>
        /// <returns>文字列リスト</returns>
        public string[] toPropertyList()
        {
            List<string> dataList = new List<string> {
                "PrimitiveId",      mPrimitiveId.ToString(),
                "PrimitiveFace",    mPrimitiveFace.ToString(),
                "LineColor",        ylib.getBrushName(mLineColor),
                "LineThickness",    mLineThickness.ToString(),
                "LineType",         mLineType.ToString(),
                "Reverse",          mReverse.ToString(),
                "DivideAngle",      mDivideAngle.ToString(),
                "FaceColors",       mFaceColors.Count.ToString(),
            };
            for (int i = 0; i < mFaceColors.Count; i++)
                dataList.Add(ylib.getBrushName(mFaceColors[i]));

            return dataList.ToArray();
        }

        /// <summary>
        /// 文字列データを設定する
        /// </summary>
        /// <param name="list">文字列リスト</param>
        public void setPropertyList(string[] list)
        {
            if (list == null || list.Length == 0)
                return;
            int ival;
            double val;
            bool bval;
            for (int i = 0; i < list.Length; i++) {
                if (list[i] == "PrimitiveId") {
                    mPrimitiveId = (PrimitiveId)Enum.Parse(typeof(PrimitiveId), list[++i]);
                } else if (list[i] == "PrimitiveFace") {
                    mPrimitiveFace = (FACE3D)Enum.Parse(typeof(FACE3D), list[++i]);
                } else if (list[i] == "LineColor") {
                    mLineColor = ylib.getBrsh(list[++i]);
                } else if (list[i] == "LineThickness") {
                    mLineThickness = double.TryParse(list[++i], out val) ? val : 1;
                } else if (list[i] == "LineType") {
                    mLineType = int.TryParse(list[++i], out ival) ? ival : 0;
                } else if (list[i] == "Reverse") {
                    mReverse = bool.TryParse(list[++i], out bval) ? bval : false;
                } else if (list[i] == "DivideAngle") {
                    mDivideAngle = double.TryParse(list[++i], out val) ? val : Math.PI / 15;
                } else if (list[i] == "FaceColors") {
                    mFaceColors.Clear();
                    int count = int.TryParse(list[++i], out ival) ? ival : 0;
                    for (int j = 0; j < count; j++) {
                        mFaceColors.Add(ylib.getBrsh(list[++i]));
                    }
                }
            }
        }

        /// Boxエリアを求める
        /// </summary>
        /// <returns></returns>
        public Box3D getArea()
        {
            if (mSurfaceDataList == null || mSurfaceDataList.Count == 0)
                return null;
            Box3D area = new Box3D(mSurfaceDataList[0].mVertexList[0]);
            for (int i = 0; i < mSurfaceDataList.Count; i++)
                mSurfaceDataList[i].mVertexList.ForEach(p => area.extension(p));
            return area;
        }
    }

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
        /// <param name="p">2D点座標</param>
        /// <param name="face">作成面</param>
        public PointPrimitive(PointD p, FACE3D face = FACE3D.XY)
        {
            mPrimitiveId = PrimitiveId.Point;
            mPrimitiveFace = face;
            mPoint = new Point3D(p, face);
            createSurfaceData();
            createVertexData();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="p">3D点座標</param>
        /// <param name="face">作成面</param>
        public PointPrimitive(Point3D p, FACE3D face = FACE3D.XY)
        {
            mPrimitiveId = PrimitiveId.Point;
            mPrimitiveFace = face;
            mPoint = p.toCopy();
            createSurfaceData();
            createVertexData();
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
        public override void translate(Point3D v)
        {
            mPoint.translate(v);
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">2D平面</param>
        public override void rotate(Point3D cp, double ang, FACE3D face)
        {
            mPoint.rotate(cp, ang, face);
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public override void offset(Point3D sp, Point3D ep, FACE3D face)
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
        public override void mirror(Point3D sp, Point3D ep, FACE3D face)
        {
            Line3D l = new Line3D(sp, ep);
            mPoint = l.mirror(mPoint);
        }

        /// <summary>
        /// トリム
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public override void trim(Point3D sp, Point3D ep, FACE3D face)
        {

        }

        /// <summary>
        /// 拡大縮小
        /// </summary>
        /// <param name="cp">拡大中心</param>
        /// <param name="scale">倍率</param>
        /// <param name="face">2D平面</param>
        public override void scale(Point3D cp, double scale, FACE3D face)
        {
            mPoint.scale(cp, scale);
        }

        /// <summary>
        /// ストレッチ
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="pickPos"></param>
        /// <param name="face"></param>
        public override void stretch(Point3D vec, PointD pickPos, bool arc, FACE3D face)
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
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">作成面</param>
        public LinePrimitive(PointD sp, PointD ep, FACE3D face = FACE3D.XY)
        {
            mPrimitiveId = PrimitiveId.Line;
            mPrimitiveFace = face;
            mLine = new Line3D(new Point3D(sp, face), new Point3D(ep, face));
            createSurfaceData();
            createVertexData();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="line">線分</param>
        /// <param name="face">作成面</param>
        public LinePrimitive(Line3D line, FACE3D face = FACE3D.XY)
        {
            mPrimitiveId = PrimitiveId.Line;
            mPrimitiveFace = face;
            mLine = line.toCopy();
            createSurfaceData();
            createVertexData();
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
        public override void translate(Point3D v)
        {
            mLine.translate(v);
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">操作面</param>
        public override void rotate(Point3D cp, double ang, FACE3D face)
        {
            mLine.rotate(cp, ang, face);
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public override void offset(Point3D sp, Point3D ep, FACE3D face)
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
        public override void mirror(Point3D sp, Point3D ep, FACE3D face)
        {
            Line3D l = new Line3D(sp, ep);
            mLine = l.mirror(mLine);
        }

        /// <summary>
        /// トリム
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        /// <param name="outline">3Dデータと外形線の作成</param>
        public override void trim(Point3D sp, Point3D ep, FACE3D face)
        {
            mLine.trim(sp, ep);
        }

        /// <summary>
        /// 拡大縮小
        /// </summary>
        /// <param name="cp">拡大中心</param>
        /// <param name="scale">倍率</param>
        /// <param name="face">2D平面</param>
        public override void scale(Point3D cp, double scale, FACE3D face)
        {
            mLine.scale(cp, scale);
        }

        /// <summary>
        /// ストレッチ
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">2D平面</param>
        public override void stretch(Point3D vec, PointD pickPos, bool arc, FACE3D face)
        {
            Point3D pos = new Point3D(pickPos, face);
            mLine.stretch(vec, pos);
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
        /// <param name="cp">中心</param>
        /// <param name="r">半径</param>
        /// <param name="color">カラー</param>
        /// <param name="face">2D平面</param>
        /// <param name="divideAng">分割角度</param>
        public ArcPrimitive(PointD cp, double r, Brush color, FACE3D face = FACE3D.XY, double divideAng = Math.PI /15)
        {
            mPrimitiveId = PrimitiveId.Arc;
            mLineColor = color;
            mFaceColors[0] = color;
            mPrimitiveFace = face;
            mArc = new Arc3D(new Point3D(cp, face), r, face);
            mDivideAngle = divideAng;
            createSurfaceData();
            createVertexData();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="arc">2D円弧</param>
        /// <param name="face">2D平面</param>
        /// <param name="divideAng">分割角度</param>
        public ArcPrimitive(ArcD arc, FACE3D face = FACE3D.XY, double divideAng = Math.PI / 15)
        {
            mPrimitiveId = PrimitiveId.Arc;
            mPrimitiveFace = face;
            mArc = new Arc3D(arc, face);
            mDivideAngle = divideAng;
            createSurfaceData();
            createVertexData();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="arc">2D円弧</param>
        /// <param name="color">カラー</param>
        /// <param name="face">2D平面</param>
        /// <param name="divideAng">分割角度</param>
        public ArcPrimitive(ArcD arc, Brush color, FACE3D face = FACE3D.XY, double divideAng = Math.PI / 15)
        {
            mPrimitiveId = PrimitiveId.Arc;
            mLineColor = color;
            mFaceColors[0] = color;
            mPrimitiveFace = face;
            mArc = new Arc3D(arc, face);
            mDivideAngle = divideAng;
            createSurfaceData();
            createVertexData();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="arc">3D円弧</param>
        /// <param name="color">カラー</param>
        /// <param name="face">2D平面</param>
        /// <param name="divideAng">分割角度</param>
        public ArcPrimitive(Arc3D arc, Brush color, FACE3D face = FACE3D.XY, double divideAng = Math.PI / 15)
        {
            mPrimitiveId = PrimitiveId.Arc;
            mLineColor = color;
            mFaceColors[0] = color;
            mPrimitiveFace = face;
            mArc = arc.toCopy();
            mDivideAngle = divideAng;
            createSurfaceData();
            createVertexData();
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
        public override void translate(Point3D v)
        {
            mArc.translate(v);
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">表示面</param>
        public override void rotate(Point3D cp, double ang, FACE3D face)
        {
            mArc.rotate(cp, ang, face);
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public override void offset(Point3D sp, Point3D ep, FACE3D face)
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
        public override void mirror(Point3D sp, Point3D ep, FACE3D face)
        {
            Line3D l = new Line3D(sp, ep);
            Point3D cp = l.mirror(mArc.mCp);
            Point3D u = l.mirror(mArc.mCp + mArc.mU);
            Point3D v = l.mirror(mArc.mCp + mArc.mV);
            mArc.mCp = cp.toCopy();
            mArc.mU = u - cp;
            mArc.mV = v - cp;
        }

        /// <summary>
        /// トリム
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public override void trim(Point3D sp, Point3D ep, FACE3D face)
        {
            mArc.trim(sp.toPoint(face), ep.toPoint(face), face);
        }

        /// <summary>
        /// 拡大縮小
        /// </summary>
        /// <param name="cp">拡大中心</param>
        /// <param name="scale">倍率</param>
        /// <param name="face">2D平面</param>
        public override void scale(Point3D cp, double scale, FACE3D face)
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
        public override void stretch(Point3D vec, PointD pickPos, bool arc, FACE3D face)
        {
            mArc.stretch(vec, new Point3D(pickPos, face));
        }


        /// <summary>
        /// 固有データを文字列配列に変換
        /// </summary>
        /// <returns>文字列配列</returns>
        public override string[] toDataList()
        {
            List<string> dataList = new List<string>() {
                "ArcData",
                "Cp", mArc.mCp.x.ToString(), mArc.mCp.y.ToString(), mArc.mCp.z.ToString(),
                "R", mArc.mR.ToString(),
                "U", mArc.mU.x.ToString(), mArc.mU.y.ToString(), mArc.mU.z.ToString(),
                "V", mArc.mV.x.ToString(), mArc.mV.y.ToString(), mArc.mV.z.ToString(),
                "Sa", mArc.mSa.ToString(), "Ea", mArc.mEa.ToString()
            };
            return dataList.ToArray();
        }

        /// <summary>
        /// 文字列配列から固有データを設定
        /// </summary>
        /// <param name="list">文字列配列</param>
        public override void setDataList(string[] list)
        {
            if (0 == list.Length || list[0] != "ArcData")
                return;
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
        /// <param name="points">2D座標リスト</param>
        /// <param name="face">2D平面</param>
        public PolylinePrimitive(List<PointD> points, Brush color, FACE3D face = FACE3D.XY)
        {
            mPrimitiveId = PrimitiveId.Polyline;
            mLineColor = color;
            mFaceColors[0] = color;
            mPrimitiveFace = face;
            mPolyline = new Polyline3D(points, face);
            mPolyline.squeeze();
            createSurfaceData();
            createVertexData();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="polyline">2Dポリライン</param>
        /// <param name="face">2D平面</param>
        public PolylinePrimitive(PolylineD polyline, FACE3D face = FACE3D.XY)
        {
            mPrimitiveId = PrimitiveId.Polyline;
            mPrimitiveFace = face;
            mPolyline = new Polyline3D(polyline, face);
            mPolyline.squeeze();
            createSurfaceData();
            createVertexData();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="points">3D座標リスト</param>
        /// <param name="color">カラー</param>
        /// <param name="face">2D平面</param>
        public PolylinePrimitive(List<Point3D> points, Brush color, FACE3D face = FACE3D.XY)
        {
            mPrimitiveId = PrimitiveId.Polyline;
            mLineColor = color;
            mFaceColors[0] = color;
            mPrimitiveFace = face;
            mPolyline = new Polyline3D(points);
            mPolyline.squeeze();
            createSurfaceData();
            createVertexData();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="polyline">ポリライン</param>
        /// <param name="color">カラー</param>
        /// <param name="face">2D平面</param>
        public PolylinePrimitive(Polyline3D polyline, Brush color, FACE3D face = FACE3D.XY)
        {
            mPrimitiveId = PrimitiveId.Polyline;
            mLineColor = color;
            mFaceColors[0] = color;
            mPrimitiveFace = face;
            mPolyline = polyline.toCopy();
            mPolyline.squeeze();
            createSurfaceData();
            createVertexData();
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
        public override void translate(Point3D v)
        {
            mPolyline.translate(v);
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">表示面</param>
        public override void rotate(Point3D cp, double ang, FACE3D face)
        {
            mPolyline.rotate(cp, ang, face);
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public override void offset(Point3D sp, Point3D ep, FACE3D face)
        {
            mPolyline.offset(sp, ep);
        }

        /// <summary>
        /// 座標を反転する
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public override void mirror(Point3D sp, Point3D ep, FACE3D face)
        {
            mPolyline.mirror(sp, ep);
        }

        /// <summary>
        /// トリム
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public override void trim(Point3D sp, Point3D ep, FACE3D face)
        {
            mPolyline.trim(sp, ep);
        }

        /// <summary>
        /// 拡大縮小
        /// </summary>
        /// <param name="cp">拡大中心</param>
        /// <param name="scale">倍率</param>
        /// <param name="face">2D平面</param>
        public override void scale(Point3D cp, double scale, FACE3D face)
        {
            mPolyline.scale(cp, scale);
        }

        /// <summary>
        /// ストレッチ
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">2D平面</param>
        public override void stretch(Point3D vec, PointD pickPos, bool arc, FACE3D face)
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
                    } else {
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
        /// <param name="points">2D座標リスト</param>
        /// <param name="color">色</param>
        /// <param name="face">作成面</param>
        public PolygonPrimitive(List<PointD> points, Brush color, FACE3D face = FACE3D.XY)
        {
            mPrimitiveId = PrimitiveId.Polygon;
            mLineColor = color;
            mFaceColors[0] = color;
            mPrimitiveFace = face;
            mPolygon = new Polygon3D(points, face);
            mPolygon.squeeze();
            if (mPolygon.isClockwise(face))
                mPolygon.reverse();
            createSurfaceData();
            createVertexData();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="polygon">2Dポリゴン</param>
        /// <param name="face">2D平面</param>
        public PolygonPrimitive(PolygonD polygon, FACE3D face = FACE3D.XY)
        {
            mPrimitiveId = PrimitiveId.Polygon;
            mPrimitiveFace = face;
            mPolygon = new Polygon3D(polygon.mPolygon, face);
            mPolygon.squeeze();
            if (mPolygon.isClockwise(face))
                mPolygon.reverse();
            createSurfaceData();
            createVertexData();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="points">3D座標リスト</param>
        /// <param name="color">色</param>
        /// <param name="face">作成面</param>
        public PolygonPrimitive(List<Point3D> points, Brush color, FACE3D face = FACE3D.XY)
        {
            mPrimitiveId = PrimitiveId.Polygon;
            mLineColor = color;
            mFaceColors[0] = color;
            mPrimitiveFace = face;
            mPolygon = new Polygon3D(points, face);
            mPolygon.squeeze();
            if (mPolygon.isClockwise(face))
                mPolygon.reverse();
            createSurfaceData();
            createVertexData();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="polygon">3Dポリゴン</param>
        /// <param name="color">色</param>
        /// <param name="face">2D平面</param>
        public PolygonPrimitive(Polygon3D polygon, Brush color, FACE3D face = FACE3D.XY)
        {
            mPrimitiveId = PrimitiveId.Polygon;
            mLineColor = color;
            mFaceColors[0] = color;
            mPrimitiveFace = face;
            mPolygon = new Polygon3D(polygon);
            mPolygon.squeeze();
            if (mPolygon.isClockwise(face))
                mPolygon.reverse();
            createSurfaceData();
            createVertexData();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="polyline">3Dポリライン</param>
        /// <param name="color">色</param>
        /// <param name="face">2D平面</param>
        public PolygonPrimitive(Polyline3D polyline, Brush color, FACE3D face = FACE3D.XY)
        {
            mPrimitiveId = PrimitiveId.Polygon;
            mLineColor = color;
            mFaceColors[0] = color;
            mPrimitiveFace = face;
            mPolygon = new Polygon3D(polyline);
            mPolygon.squeeze();
            if (mPolygon.isClockwise(face))
                mPolygon.reverse();
            createSurfaceData();
            createVertexData();
        }

        /// <summary>
        /// 3D座標リストの作成(三角形の集合)
        /// </summary>
        public override void createSurfaceData()
        {
            mSurfaceDataList = new List<SurfaceData>();
            (List<Point3D> triangles, bool reverse) = mPolygon.cnvTriangles(mDivideAngle);
            if (triangles.Count < 3)
                return;
            SurfaceData surfaceData = new SurfaceData();
            surfaceData.mVertexList = triangles;
            surfaceData.mDrawType = DRAWTYPE.TRIANGLES;
            surfaceData.mFaceColor = mFaceColors[0];
            surfaceData.reverse(mReverse);
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
        public override void translate(Point3D v)
        {
            mPolygon.translate(v);
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">表示面</param>
        public override void rotate(Point3D cp, double ang, FACE3D face)
        {
            mPolygon.rotate(cp, ang, face);
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public override void offset(Point3D sp, Point3D ep, FACE3D face)
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
        public override void mirror(Point3D sp, Point3D ep, FACE3D face)
        {
            mPolygon.mirror(sp, ep);
        }

        /// <summary>
        /// トリム
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public override void trim(Point3D sp, Point3D ep, FACE3D face)
        {

        }

        /// <summary>
        /// 拡大縮小
        /// </summary>
        /// <param name="cp">拡大中心</param>
        /// <param name="scale">倍率</param>
        /// <param name="face">2D平面</param>
        public override void scale(Point3D cp, double scale, FACE3D face)
        {
            mPolygon.scale(cp, scale);
        }

        /// <summary>
        /// ストレッチ
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">2D平面</param>
        public override void stretch(Point3D vec, PointD pickPos, bool arc, FACE3D face)
        {
            mPolygon.stretch(vec, new Point3D(pickPos, face), arc);
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
                    } else {
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

    /// <summary>
    /// 押出プリミティブ
    /// </summary>
    public class ExtrusionPrimitive : Primitive
    {
        public Polygon3D mPolygon;
        public Point3D mVector;
        public bool mClose = true;
        public FACE3D mSrcFace;

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
        /// <param name="points">2D座標点リスト</param>
        /// <param name="srcFace">作成面</param>
        /// <param name="v">押出方向</param>
        /// <param name="color">色</param>
        /// <param name="close">閉領域</param>
        /// <param name="face">表示面</param>
        public ExtrusionPrimitive(List<PointD> points, FACE3D srcFace, Point3D v, Brush color, bool close = true, FACE3D face = FACE3D.XY)
        {
            mPrimitiveId = PrimitiveId.Extrusion;
            mPrimitiveFace = face;
            mPolygon = new Polygon3D(points, srcFace);
            mPolygon.squeeze();
            mVector = v;
            mLineColor = color;
            mFaceColors[0] = color;
            mClose = close;
            mSrcFace = srcFace;
            createSurfaceData();
            createVertexData();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="points">3D座標点リスト</param>
        /// <param name="srcFace">作成面</param>
        /// <param name="v">押出方向</param>
        /// <param name="color">色</param>
        /// <param name="close">閉領域</param>
        /// <param name="face">表示面</param>
        public ExtrusionPrimitive(List<Point3D> points, FACE3D srcFace, Point3D v, Brush color, bool close = true, FACE3D face = FACE3D.XY)
        {
            mPrimitiveId = PrimitiveId.Extrusion;
            mPrimitiveFace = face;
            mPolygon = new Polygon3D(points);
            mPolygon.squeeze();
            mVector = v;
            mLineColor = color;
            mFaceColors[0] = color;
            mClose = close;
            mSrcFace = srcFace;
            createSurfaceData();
            createVertexData();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="polygon">ポリゴン</param>
        /// <param name="srcFace">参照データの作成面</param>
        /// <param name="v">押出ベクトル</param>
        /// <param name="color">色</param>
        /// <param name="close">閉領域</param>
        /// <param name="face">作成面</param>
        public ExtrusionPrimitive(Polygon3D polygon, FACE3D srcFace, Point3D v, Brush color, bool close = true, FACE3D face = FACE3D.XY)
        {
            mPrimitiveId = PrimitiveId.Extrusion;
            mPrimitiveFace = face;
            mPolygon = polygon.toCopy();
            mPolygon.squeeze();
            mVector = v;
            mLineColor = color;
            mFaceColors[0] = color;
            mClose = close;
            mSrcFace = srcFace;
            createSurfaceData();
            createVertexData();
        }

        /// <summary>
        /// 3D座標リストの作成
        /// </summary>
        public override void createSurfaceData()
        {
            mSurfaceDataList = new List<SurfaceData>();
            SurfaceData surfaceData;
            Point3D normal = mPolygon.getNormalLine();
            bool wise = (Math.PI / 2) > normal.angle(mVector);
            if (mClose) {
                //  1面(端面)
                surfaceData = new SurfaceData();
                (surfaceData.mVertexList, bool reverse) = mPolygon.cnvTriangles(mDivideAngle);
                Point3D v0 = surfaceData.mVertexList[0].getNormal(surfaceData.mVertexList[1], surfaceData.mVertexList[2]);
                if (v0.angle(mVector) < Math.PI / 2) {
                    //if (wise)
                    surfaceData.mVertexList.Reverse();
                    //wise = true;
                }
                surfaceData.mDrawType = DRAWTYPE.TRIANGLES;
                surfaceData.mFaceColor = mFaceColors[0];
                surfaceData.reverse(mReverse);
                mSurfaceDataList.Add(surfaceData);
                //  2面(端面)
                surfaceData = new SurfaceData();
                surfaceData.mVertexList = mSurfaceDataList[0].mVertexList.ConvertAll(p => p.toCopy());
                surfaceData.mVertexList.ForEach(p => p.translate(mVector));
                surfaceData.mVertexList.Reverse();
                surfaceData.mDrawType = DRAWTYPE.TRIANGLES;
                surfaceData.mFaceColor = mFaceColors[0];
                surfaceData.reverse(mReverse);
                mSurfaceDataList.Add(surfaceData);
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
            if (mClose) {
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

        /// <summary>
        /// 2D表示用座標リストの作成
        /// </summary>
        public override void createVertexData()
        {
            mVertexList = new List<List<Point3D>>();
            //  1面(端面)
            Polygon3D polygon1 = mPolygon.toCopy();
            mVertexList.Add(polygon1.toPoint3D(mDivideAngle, mClose));
            if (!mSurfaceVertex) {
                mVertexList.Add(polygon1.toPoint3D(mDivideAngle, mClose));
            } else {
                //  Debug(Surface表示確認)
                mVertexList.AddRange(getVertexList(polygon1));
            }
            //  2面(端面)
            Polygon3D polygon2 = mPolygon.toCopy();
            polygon2.translate(mVector);
            mVertexList.Add(polygon2.toPoint3D(mDivideAngle, mClose));
            if (!mSurfaceVertex) {
                mVertexList.Add(polygon2.toPoint3D(mDivideAngle, mClose));
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
        public override void translate(Point3D v)
        {
            mPolygon.translate(v);
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">表示面</param>
        public override void rotate(Point3D cp, double ang, FACE3D face)
        {
            mVector.rotate(new Point3D(0, 0, 0), ang, face);
            mPolygon.rotate(cp, ang, face);
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public override void offset(Point3D sp, Point3D ep, FACE3D face)
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
        public override void mirror(Point3D sp, Point3D ep, FACE3D face)
        {
            Line3D l = new Line3D(new Point3D(0, 0, 0), ep - sp);
            mVector = l.mirror(mVector);
            mPolygon.mirror(sp, ep);
        }

        /// <summary>
        /// トリム
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public override void trim(Point3D sp, Point3D ep, FACE3D face)
        {

        }

        /// <summary>
        /// 拡大縮小
        /// </summary>
        /// <param name="cp">拡大中心</param>
        /// <param name="scale">倍率</param>
        /// <param name="face">2D平面</param>
        public override void scale(Point3D cp, double scale, FACE3D face)
        {
            mVector.length(mVector.length() * scale);
            mPolygon.scale(cp, scale);
        }

        public override void stretch(Point3D vec, PointD pickPos, bool arc, FACE3D face)
        {

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
                "SrcFace", mSrcFace.ToString(), "Close", mClose.ToString(),
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
                    } else if (list[i] == "SrcFace") {
                        mSrcFace = (FACE3D)Enum.Parse(typeof(FACE3D), list[++i]);
                    } else if (list[i] == "Close") {
                        mClose = bool.TryParse(list[++i], out bval) ? bval : true;
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
                    } else {
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
            buf += " Close " + mClose;
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
            extrusion.mClose = mClose;
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

    /// <summary>
    /// 回転体クラス
    /// </summary>
    public class RevolutionPrimitive : Primitive
    {
        public Line3D mCenterLine;
        public Polyline3D mOutLine;
        public double mSa = 0;
        public double mEa = Math.PI * 2;
        public bool mClose = false;

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
        public RevolutionPrimitive(Line3D centerLine, Polyline3D outLine, Brush color, double divideAngle = Math.PI / 16, bool close = true, FACE3D face = FACE3D.XY)
        {
            mPrimitiveId = PrimitiveId.Revolution;
            mPrimitiveFace = face;
            mLineColor = color;
            mFaceColors[0] = color;
            mCenterLine = centerLine.toCopy();
            mOutLine = outLine.toCopy();
            mDivideAngle = divideAngle;
            mClose = close;
            createSurfaceData();
            createVertexData();
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
            if (mClose) {
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

        /// <summary>
        /// 2D表示用座標リストの作成
        /// </summary>
        public override void createVertexData()
        {
            mVertexList = new List<List<Point3D>>();
            List<List<Point3D>> outLines;
            double divideAngle = mDivideAngle < (Math.PI / 6) ? mDivideAngle * 2 : mDivideAngle;
            outLines = getCenterLineRotate(mCenterLine, mOutLine.toPoint3D(), divideAngle);
            mVertexList.AddRange(outLines);
            for (int i = 0; i < outLines[0].Count; i++) {
                List<Point3D> plist = new List<Point3D>();
                for (int j = 0; j < outLines.Count; j++) {
                    plist.Add(outLines[j][i]);
                }
                //plist.Add(outLines[0][i]);
                mVertexList.Add(plist);
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
        public override void translate(Point3D v)
        {
            mCenterLine.translate(v);
            mOutLine.translate(v);
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">表示面</param>
        public override void rotate(Point3D cp, double ang, FACE3D face)
        {
            mCenterLine.rotate(cp, ang, face);
            mOutLine.rotate(cp, ang, face);
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public override void offset(Point3D sp, Point3D ep, FACE3D face)
        {
            mOutLine.offset(sp, ep);
        }

        /// <summary>
        /// 座標を反転する
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public override void mirror(Point3D sp, Point3D ep, FACE3D face)
        {
            Line3D l = new Line3D(sp, ep);
            mCenterLine = l.mirror(mCenterLine);
            mOutLine.mirror(sp, ep);
        }

        /// <summary>
        /// トリム
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public override void trim(Point3D sp, Point3D ep, FACE3D face)
        {

        }

        /// <summary>
        /// 拡大縮小
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="scale"></param>
        /// <param name="face"></param>
        public override void scale(Point3D cp, double scale, FACE3D face)
        {
            mCenterLine.scale(cp, scale);
            mOutLine.scale(cp, scale);
        }

        public override void stretch(Point3D vec, PointD pickPos, bool arc, FACE3D face)
        {

        }

        /// <summary>
        /// 固有データを文字列配列に変換
        /// </summary>
        /// <returns>文字列配列</returns>
        public override string[] toDataList()
        {
            List<string> dataList = new List<string>() {
                "RevolutionData",
                "StartAngle", mSa.ToString(),
                "EndAngle", mEa.ToString(),
                "Close", mClose.ToString(),
                "CenterLineSp", mCenterLine.mSp.x.ToString(), mCenterLine.mSp.y.ToString(), mCenterLine.mSp.z.ToString(),
                "CenterLineV", mCenterLine.mV.x.ToString(), mCenterLine.mV.y.ToString(), mCenterLine.mV.z.ToString(),
                "OutLineCp", mOutLine.mCp.x.ToString(), mOutLine.mCp.y.ToString(), mOutLine.mCp.z.ToString(),
                "OutLineU", mOutLine.mU.x.ToString(), mOutLine.mU.y.ToString(), mOutLine.mU.z.ToString(),
                "OutLineV", mOutLine.mV.x.ToString(), mOutLine.mV.y.ToString(), mOutLine.mV.z.ToString(),
                "OutLineSize", mOutLine.mPolyline.Count.ToString(),
                "OutLine"
            };
            for (int i = 0; i < mOutLine.mPolyline.Count; i++) {
                dataList.Add(mOutLine.mPolyline[i].x.ToString());
                dataList.Add(mOutLine.mPolyline[i].y.ToString());
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
                int count =0;
                while (i < list.Length) {
                    if (list[i] == "StartAngle") {
                        mSa = double.TryParse(list[++i], out val) ? val : 0;
                    } else if (list[i] == "EndAngle") {
                        mEa = double.TryParse(list[++i], out val) ? val : 0;
                    } else if (list[i] == "Close") {
                        mClose = bool.TryParse(list[++i], out bval) ? bval : true;
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
                    } else if (list[i] == "OutLine") {
                        for (int j = 0; j < count; j++) {
                            PointD p = new PointD();
                            p.x = double.TryParse(list[++i], out val) ? val : 0;
                            p.y = double.TryParse(list[++i], out val) ? val : 0;
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
            buf += " Close " + mClose.ToString();
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

    /// <summary>
    /// スィープ
    /// </summary>
    public class SweepPrimitive : Primitive
    {

        public Polyline3D mOutLine1;
        public Polyline3D mOutLine2;
        public double mSa = 0;
        public double mEa = Math.PI * 2;
        public bool mClose = false;

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
        public SweepPrimitive(Polyline3D outline1, Polyline3D outline2, Brush color, 
            double divideAngle = Math.PI / 9, bool close = true, FACE3D face = FACE3D.XY)
        {
            mPrimitiveId = PrimitiveId.Sweep;
            mPrimitiveFace = face;
            mLineColor = color;
            mFaceColors[0] = color;
            mOutLine1 = outline1.toCopy();
            mOutLine2 = outline2.toCopy();
            mDivideAngle = divideAngle;
            mClose = close;
            createSurfaceData();
            createVertexData();
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
            if (!directChk(mOutLine1, mOutLine2, mPrimitiveFace))
                mOutLine2.mPolyline.Reverse();
            outLines = rotateOutlines(mOutLine1, mOutLine2, mDivideAngle);
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
            if (mClose) {
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

        /// <summary>
        /// 2D表示用座標リストの作成
        /// </summary>
        public override void createVertexData()
        {
            mVertexList = new List<List<Point3D>>();
            List<List<Point3D>> outLines;
            if (!directChk(mOutLine1, mOutLine2, mPrimitiveFace))
                mOutLine2.mPolyline.Reverse();
            double divideAngle = mDivideAngle < (Math.PI / 6) ? mDivideAngle * 2 : mDivideAngle;
            outLines = rotateOutlines(mOutLine1, mOutLine2, divideAngle);
            mVertexList.AddRange(outLines);
            for (int j = 0; j < outLines[0].Count; j++) {
                List<Point3D> plist = new List<Point3D>();
                for (int i = 0; i < outLines.Count; i++) {
                    plist.Add(outLines[i][j].toCopy());
                }
                //plist.Add(outLines[0][j].toCopy());
                mVertexList.Add(plist);
            }
        }

        /// <summary>
        /// 外形線同士の方向チェック
        /// </summary>
        /// <param name="outline1">外形線1</param>
        /// <param name="outline2">外形線2</param>
        /// <param name="face">作成2D平面</param>
        /// <returns></returns>
        private bool directChk(Polyline3D outline1, Polyline3D outline2, FACE3D face)
        {
            PointD sp1 = outline1.toFirstPoint3D().toPoint(face);
            PointD ep1 = outline1.toLastPoint3D().toPoint(face);
            PointD sp2 = outline2.toFirstPoint3D().toPoint(face);
            PointD ep2 = outline2.toLastPoint3D().toPoint(face);
            LineD l1 = new LineD(sp1, sp2);
            LineD l2 = new LineD(ep1, ep2);
            PointD ip = l1.intersection(l2);
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
            (List<Line3D> centerlines, List<Line3D> outlines) = getCenterlines(outline1 , outline2);
            double ang = mSa;
            double dang = divideAngle;
            while (ang < mEa) {
                List<Point3D> plist = new List<Point3D>();
                for (int i  = 0; i < centerlines.Count; i++) {
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
        public override void translate(Point3D v)
        {
            mOutLine1.translate(v);
            mOutLine2.translate(v);
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">表示面</param>
        public override void rotate(Point3D cp, double ang, FACE3D face)
        {
            mOutLine1.rotate(cp, ang, face);
            mOutLine2.rotate(cp, ang, face);
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public override void offset(Point3D sp, Point3D ep, FACE3D face)
        {
            double d = ep.length(sp);
            mOutLine1.offset(d);
            mOutLine2.offset(d);
        }

        /// <summary>
        /// 座標を反転する
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        /// <param name="outline">3Dデータと外形線の作成</param>
        public override void mirror(Point3D sp, Point3D ep, FACE3D face)
        {
            mOutLine1.mirror(sp, ep);
            mOutLine2.mirror(sp, ep);
        }

        /// <summary>
        /// トリム
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public override void trim(Point3D sp, Point3D ep, FACE3D face)
        {

        }

        /// <summary>
        /// 拡大縮小
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="scale"></param>
        /// <param name="face"></param>
        public override void scale(Point3D cp, double scale, FACE3D face)
        {
            mOutLine1.scale(cp, scale);
            mOutLine2.scale(cp, scale);
        }

        public override void stretch(Point3D vec, PointD pickPos, bool arc, FACE3D face)
        {

        }

        /// <summary>
        /// 固有データを文字列配列に変換
        /// </summary>
        /// <returns>文字列配列</returns>
        public override string[] toDataList()
        {
            List<string> dataList = new List<string>() {
                "SweepData",
                "StartAngle", mSa.ToString(),
                "EndAngle", mEa.ToString(),
                "Close", mClose.ToString(),
                "OutLine1Cp", mOutLine1.mCp.x.ToString(), mOutLine1.mCp.y.ToString(), mOutLine1.mCp.z.ToString(),
                "OutLine1U", mOutLine1.mU.x.ToString(), mOutLine1.mU.y.ToString(), mOutLine1.mU.z.ToString(),
                "OutLine1V", mOutLine1.mV.x.ToString(), mOutLine1.mV.y.ToString(), mOutLine1.mV.z.ToString(),
                "OutLine1Size", mOutLine1.mPolyline.Count.ToString(),
                "OutLine1"
            };
            for (int i = 0; i < mOutLine1.mPolyline.Count; i++) {
                dataList.Add(mOutLine1.mPolyline[i].x.ToString());
                dataList.Add(mOutLine1.mPolyline[i].y.ToString());
            }
            List<string> buf = new List<string>() {
                "OutLine2Cp", mOutLine2.mCp.x.ToString(), mOutLine2.mCp.y.ToString(), mOutLine2.mCp.z.ToString(),
                "OutLine2U", mOutLine2.mU.x.ToString(), mOutLine2.mU.y.ToString(), mOutLine2.mU.z.ToString(),
                "OutLine2V", mOutLine2.mV.x.ToString(), mOutLine2.mV.y.ToString(), mOutLine2.mV.z.ToString(),
                "OutLine2Size", mOutLine2.mPolyline.Count.ToString(),
                "OutLine2"
            };
            dataList.AddRange(buf);
            for (int i = 0; i < mOutLine2.mPolyline.Count; i++) {
                dataList.Add(mOutLine2.mPolyline[i].x.ToString());
                dataList.Add(mOutLine2.mPolyline[i].y.ToString());
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
                while (i < list.Length) {
                    if (list[i] == "StartAngle") {
                        mSa = double.TryParse(list[++i], out val) ? val : 0;
                    } else if (list[i] == "EndAngle") {
                        mEa = double.TryParse(list[++i], out val) ? val : 0;
                    } else if (list[i] == "Close") {
                        mClose = bool.TryParse(list[++i], out bval) ? bval : true;
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
                    } else if (list[i] == "OutLine1") {
                        for (int j = 0; j < count; j++) {
                            PointD p = new PointD();
                            p.x = double.TryParse(list[++i], out val) ? val : 0;
                            p.y = double.TryParse(list[++i], out val) ? val : 0;
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
                    } else if (list[i] == "OutLine2") {
                        for (int j = 0; j < count; j++) {
                            PointD p = new PointD();
                            p.x = double.TryParse(list[++i], out val) ? val : 0;
                            p.y = double.TryParse(list[++i], out val) ? val : 0;
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
            buf += " Close " + mClose.ToString();
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

