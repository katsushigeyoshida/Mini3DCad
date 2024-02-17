using CoreLib;
using System.Windows.Shapes;
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
        Line, Arc, Polyline, Polygon,
        WireCube, Cube, Cylinder, Sphere, Cone,
        Extrusion, Revolution, Sweep
    }

    /// <summary>
    /// 描画方式
    /// </summary>
    public enum DRAWTYPE
    {
        LINES, LINE_STRIP, LINE_LOOP,
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
        public abstract void translate(Point3D v, bool outline = false);

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">操作面</param>
        public abstract void rotate(Point3D cp, double ang, FACE3D face, bool outline = false);

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">操作面</param>
        public abstract void offset(Point3D sp, Point3D ep, FACE3D face, bool outline = false);

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
        /// コピーを作成
        /// </summary>
        public abstract Primitive toCopy();

        /// <summary>
        /// 座標点リストをPolyline3Dで取得
        /// </summary>
        /// <returns></returns>
        public abstract Polyline3D getVertexList();

        /// <summary>
        /// 2D平面上で最も近い分割点の座標を求める
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="divideNo"></param>
        /// <param name="face"></param>
        /// <returns></returns>
        public abstract PointD nearPoint(PointD pos, int divideNo, FACE3D face);

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
        public void copyProperty(Primitive primitive, bool dataList = false)
        {
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
            for (int i = 0; i < mVertexList.Count; i++) {
                List<PointD> plist = mVertexList[i].ConvertAll(p => p.toPoint(face));
                draw.drawWPolyline(plist);
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
        /// 属性情報
        /// </summary>
        /// <returns></returns>
        public string propertyInfo()
        {
            string buf = $"ID: {mPrimitiveId}, Face: {mPrimitiveFace},";
            buf += $" LineColor:{ylib.getBrushName(mLineColor)}, FaceColor:{ylib.getBrushName(mFaceColors[0])},";
            buf += $" LineType:{mLineType}, Thickness:{mLineThickness}";
            return buf;
        }

        /// <summary>
        /// 3D座標情報
        /// </summary>
        /// <returns></returns>
        public List<string> vertexInfo(string form = "F2")
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
                "LineType",        mLineType.ToString(),
                "FaceColors",       mFaceColors.Count.ToString()
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
        public override void translate(Point3D v, bool outline = false)
        {
            mLine.translate(v);
            if (!outline) {
                createSurfaceData();
                createVertexData();
            }
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">操作面</param>
        public override void rotate(Point3D cp, double ang, FACE3D face, bool outline = false)
        {
            mLine.rotate(cp, ang, face);
            if (!outline) {
                createSurfaceData();
                createVertexData();
            }
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public override void offset(Point3D sp, Point3D ep, FACE3D face, bool outline = false)
        {
            Point3D v = ep - sp;
            mLine.offset(v);
            if (!outline) {
                createSurfaceData();
                createVertexData();
            }
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
                System.Diagnostics.Debug.WriteLine(e.ToString());
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
        /// コピーを作成
        /// </summary>
        public override Primitive toCopy()
        {
            LinePrimitive line = new LinePrimitive();
            line.copyProperty(this, true);
            line.mLine = mLine.toCopy();
            return line;
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
        /// 2D平面上で最も近い分割点の座標を求める
        /// </summary>
        /// <param name="pos">指定点</param>
        /// <param name="divideNo">分割数</param>
        /// <param name="face">2D平面</param>
        /// <returns>分割点座標</returns>
        public override PointD nearPoint(PointD pos, int divideNo, FACE3D face)
        {
            LineD l = mLine.toLineD(face);
            return l.nearPoint(pos, divideNo);
        }
    }

    /// <summary>
    /// 円/円弧プリミティブ
    /// </summary>
    public class ArcPrimitive : Primitive
    {
        public Arc3D mArc;
        public double mDivideAngle = Math.PI / 15;

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
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
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
        /// <param name="arc">円弧</param>
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

        public override void createVertexData()
        {
            mVertexList = new List<List<Point3D>>();
            mVertexList.Add(mArc.toPoint3D());
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public override void translate(Point3D v, bool outline = false)
        {
            mArc.translate(v);
            if (!outline) {
                createSurfaceData();
                createVertexData();
            }
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">表示面</param>
        public override void rotate(Point3D cp, double ang, FACE3D face, bool outline = false)
        {
            mArc.rotate(cp, ang, face);
            if (!outline) {
                createSurfaceData();
                createVertexData();
            }
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public override void offset(Point3D sp, Point3D ep, FACE3D face, bool outline = false)
        {
            mArc.offset(sp, ep);
            if (!outline) {
                createSurfaceData();
                createVertexData();
            }
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
                System.Diagnostics.Debug.WriteLine(e.ToString());
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
        /// コピーを作成
        /// </summary>
        public override Primitive toCopy()
        {
            ArcPrimitive arc = new ArcPrimitive();
            arc.copyProperty(this, true);
            arc.mArc = mArc.toCopy();
            arc.mDivideAngle = mDivideAngle;
            return arc;
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
        /// 2D平面上で最も近い分割点の座標を求める
        /// </summary>
        /// <param name="pos">指定点</param>
        /// <param name="divideNo">分割数</param>
        /// <param name="face">2D平面</param>
        /// <returns>分割点座標</returns>
        public override PointD nearPoint(PointD pos, int divideNo, FACE3D face)
        {
            return mArc.nearPoint(pos, divideNo, face);
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
        /// <param name="points">座標リスト</param>
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
        /// <param name="points">座標リスト</param>
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
            surfaceData.mVertexList = mPolyline.toPoint3D();
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
            mVertexList.Add(mPolyline.toPoint3D());
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public override void translate(Point3D v, bool outline = false)
        {
            mPolyline.translate(v);
            if (!outline) {
                createSurfaceData();
                createVertexData();
            }
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">表示面</param>
        public override void rotate(Point3D cp, double ang, FACE3D face, bool outline = false)
        {
            mPolyline.rotate(cp, ang, face);
            if (!outline) {
                createSurfaceData();
                createVertexData();
            }
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public override void offset(Point3D sp, Point3D ep, FACE3D face, bool outline = false)
        {
            mPolyline.offset(sp, ep);
            if (!outline) {
                createSurfaceData();
                createVertexData();
            }
        }

        /// <summary>
        /// 固有データを文字列配列に変換
        /// </summary>
        /// <returns>文字列配列</returns>
        public override string[] toDataList()
        {
            List<string> dataList = new List<string>() {
                "PolylineData",
                "Cp", mPolyline.mCp.x.ToString(), mPolyline.mCp.y.ToString(), mPolyline.mCp.z.ToString(),
                "U", mPolyline.mU.x.ToString(), mPolyline.mU.y.ToString(), mPolyline.mU.z.ToString(),
                "V", mPolyline.mV.x.ToString(), mPolyline.mV.y.ToString(), mPolyline.mV.z.ToString(),
                "Size", mPolyline.mPolyline.Count.ToString()
            };
            for (int i = 0; i < mPolyline.mPolyline.Count; i++) {
                dataList.Add(mPolyline.mPolyline[i].x.ToString());
                dataList.Add(mPolyline.mPolyline[i].y.ToString());
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
                    } else {
                        PointD p = new PointD();
                        p.x = double.TryParse(list[i], out val) ? val : 0;
                        p.y = double.TryParse(list[++i], out val) ? val : 0;
                        mPolyline.mPolyline.Add(p);
                    }
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.ToString());
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
        /// コピーを作成
        /// </summary>
        public override Primitive toCopy()
        {
            PolylinePrimitive polyline = new PolylinePrimitive();
            polyline.copyProperty(this, true);
            polyline.mPolyline = mPolyline.toCopy();
            return polyline;
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
        /// 2D平面上で最も近い分割点の座標を求める
        /// </summary>
        /// <param name="pos">指定点</param>
        /// <param name="divideNo">分割数</param>
        /// <param name="face">2D平面</param>
        /// <returns>分割点座標</returns>
        public override PointD nearPoint(PointD pos, int divideNo, FACE3D face)
        {
            return mPolyline.nearPoint(pos, divideNo, face);
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
        /// <param name="points">座標リスト</param>
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
        /// <param name="points">座標リスト</param>
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
        /// <param name="polyline">ポリライン</param>
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
            (List<Point3D> triangles, bool reverse) = mPolygon.cnvTriangles();
            if (triangles.Count < 3)
                return;
            SurfaceData surfaceData = new SurfaceData();
            surfaceData.mVertexList = triangles;
            surfaceData.mDrawType = DRAWTYPE.TRIANGLES;
            surfaceData.mFaceColor = mFaceColors[0];
            mSurfaceDataList.Add(surfaceData);
        }

        /// <summary>
        /// 2D表示用座標リストの作成
        /// </summary>
        public override void createVertexData()
        {
            mVertexList = new List<List<Point3D>>();
            mVertexList.Add(mPolygon.toPoint3D(true));
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public override void translate(Point3D v, bool outline = false)
        {
            mPolygon.translate(v);
            if (!outline) {
                createSurfaceData();
                createVertexData();
            }
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">表示面</param>
        public override void rotate(Point3D cp, double ang, FACE3D face, bool outline = false)
        {
            mPolygon.rotate(cp, ang, face);
            if (!outline) {
                createSurfaceData();
                createVertexData();
            }
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public override void offset(Point3D sp, Point3D ep, FACE3D face, bool outline = false)
        {
            mPolygon.offset(sp, ep);
            if (!outline) {
                createSurfaceData();
                createVertexData();
            }
        }

        /// <summary>
        /// 固有データを文字列配列に変換
        /// </summary>
        /// <returns>文字列配列</returns>
        public override string[] toDataList()
         {
            List<string> dataList = new List<string>() {
                "PolygonData",
                "Cp", mPolygon.mCp.x.ToString(), mPolygon.mCp.y.ToString(), mPolygon.mCp.z.ToString(),
                "U", mPolygon.mU.x.ToString(), mPolygon.mU.y.ToString(), mPolygon.mU.z.ToString(),
                "V", mPolygon.mV.x.ToString(), mPolygon.mV.y.ToString(), mPolygon.mV.z.ToString(),
                "Size", mPolygon.mPolygon.Count.ToString()
            };
            for (int i = 0; i < mPolygon.mPolygon.Count; i++) {
                dataList.Add(mPolygon.mPolygon[i].x.ToString());
                dataList.Add(mPolygon.mPolygon[i].y.ToString());
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
                    } else {
                        PointD p = new PointD();
                        p.x = double.TryParse(list[i], out val) ? val : 0;
                        p.y = double.TryParse(list[++i], out val) ? val : 0;
                        mPolygon.mPolygon.Add(p);
                    }
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.ToString());
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
        /// コピーを作成
        /// </summary>
        public override Primitive toCopy()
        {
            PolygonPrimitive polygon = new PolygonPrimitive();
            polygon.copyProperty(this, true);
            polygon.mPolygon = mPolygon.toCopy();
            return polygon;
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
        /// 2D平面上で最も近い分割点の座標を求める
        /// </summary>
        /// <param name="pos">指定点</param>
        /// <param name="divideNo">分割数</param>
        /// <param name="face">2D平面</param>
        /// <returns>分割点座標</returns>
        public override PointD nearPoint(PointD pos, int divideNo, FACE3D face)
        {
            return mPolygon.nearPoint(pos, divideNo, face);
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
            if (mClose) {
                //  1面(端面)
                surfaceData = new SurfaceData();
                (surfaceData.mVertexList, bool reverse) = mPolygon.cnvTriangles();
                Point3D v0 = surfaceData.mVertexList[0].getNormal(surfaceData.mVertexList[1], surfaceData.mVertexList[2]);
                if (v0.angle(mVector) < Math.PI / 2)
                    surfaceData.mVertexList.Reverse();
                surfaceData.mDrawType = DRAWTYPE.TRIANGLES;
                surfaceData.mFaceColor = mFaceColors[0];
                mSurfaceDataList.Add(surfaceData);
                //  2面(端面)
                surfaceData = new SurfaceData();
                surfaceData.mVertexList = mSurfaceDataList[0].mVertexList.ConvertAll(p => p.toCopy());
                surfaceData.mVertexList.ForEach(p => p.translate(mVector));
                surfaceData.mVertexList.Reverse();
                surfaceData.mDrawType = DRAWTYPE.TRIANGLES;
                surfaceData.mFaceColor = mFaceColors[0];
                mSurfaceDataList.Add(surfaceData);
            }
            //  側面
            surfaceData = new SurfaceData();
            surfaceData.mVertexList = new List<Point3D>();
            List<Point3D> outline = mPolygon.toPoint3D();
            for (int i = 0; i < outline.Count; i++) {
                surfaceData.mVertexList.Add(outline[i]);
                Point3D np = outline[i].toCopy();
                np.translate(mVector);
                surfaceData.mVertexList.Add(np);
            }
            if (mClose) {
                surfaceData.mVertexList.Add(outline[0]);
                Point3D np = outline[0].toCopy();
                np.translate(mVector);
                surfaceData.mVertexList.Add(np);
            }
            surfaceData.mDrawType = DRAWTYPE.QUAD_STRIP;
            surfaceData.mFaceColor = mFaceColors[0];
            mSurfaceDataList.Add(surfaceData);
        }

        /// <summary>
        /// 2D表示用座標リストの作成
        /// </summary>
        public override void createVertexData()
        {
            mVertexList = new List<List<Point3D>>();
            Polygon3D polygon1 = mPolygon.toCopy();
            mVertexList.Add(polygon1.toPoint3D(true));
            Polygon3D polygon2 = mPolygon.toCopy();
            polygon2.translate(mVector);
            mVertexList.Add(polygon2.toPoint3D());
            for (int i = 0; i < polygon1.mPolygon.Count; i++) {
                Point3D p1 = polygon1.toPoint3D(i);
                Point3D p2 = p1.toCopy();
                p2.add(mVector);
                List<Point3D> plist = new List<Point3D>() { p1, p2 };
                mVertexList.Add(plist);
            }
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public override void translate(Point3D v, bool outline = false)
        {
            mPolygon.translate(v);
            if (!outline) {
                createSurfaceData();
                createVertexData();
            }
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">表示面</param>
        public override void rotate(Point3D cp, double ang, FACE3D face, bool outline = false)
        {
            mVector.rotate(new Point3D(0, 0, 0), ang, face);
            mPolygon.rotate(cp, ang, face);
            if (!outline) {
                createSurfaceData();
                createVertexData();
            }
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public override void offset(Point3D sp, Point3D ep, FACE3D face, bool outline = false)
        {

        }

        /// <summary>
        /// 固有データを文字列配列に変換
        /// </summary>
        /// <returns>文字列配列</returns>
        public override string[] toDataList()
        {
            List<string> dataList = new List<string>() {
                "ExtrusionData", "Vector", mVector.x.ToString(), mVector.y.ToString(), mVector.z.ToString(),
                "SrcFace", mSrcFace.ToString(), "Close", mClose.ToString(),
                "Cp", mPolygon.mCp.x.ToString(), mPolygon.mCp.y.ToString(), mPolygon.mCp.z.ToString(),
                "U", mPolygon.mU.x.ToString(), mPolygon.mU.y.ToString(), mPolygon.mU.z.ToString(),
                "V", mPolygon.mV.x.ToString(), mPolygon.mV.y.ToString(), mPolygon.mV.z.ToString(),
                "Size", mPolygon.mPolygon.Count.ToString()
            };
            for (int i = 0; i < mPolygon.mPolygon.Count; i++) {
                dataList.Add(mPolygon.mPolygon[i].x.ToString());
                dataList.Add(mPolygon.mPolygon[i].y.ToString());
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
                    } else {
                        PointD p = new PointD();
                        p.x = double.TryParse(list[i], out val) ? val : 0;
                        p.y = double.TryParse(list[++i], out val) ? val : 0;
                        mPolygon.mPolygon.Add(p);
                    }
                    i++;
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.ToString());
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
            buf += " Polygon ";
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
        /// コピーを作成
        /// </summary>
        public override Primitive toCopy()
        {
            ExtrusionPrimitive extrusion = new ExtrusionPrimitive();
            extrusion.copyProperty(this, true);
            extrusion.mPolygon = mPolygon.toCopy();
            extrusion.mVector = mVector.toCopy();
            extrusion.mClose = mClose;
            return extrusion;
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
        /// ダミー
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="divideNo"></param>
        /// <param name="face"></param>
        /// <returns></returns>
        public override PointD nearPoint(PointD pos, int divideNo, FACE3D face)
        {
            return null;
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
        public double mDivideAngle = Math.PI / 16;
        public bool mClose = true;

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
        public RevolutionPrimitive(Line3D centerLine, Polyline3D outLine, Brush color, bool close = true, FACE3D face = FACE3D.XY)
        {
            mPrimitiveId = PrimitiveId.Revolution;
            mPrimitiveFace = face;
            mLineColor = color;
            mFaceColors[0] = color;
            mCenterLine = centerLine.toCopy();
            mOutLine = outLine.toCopy();
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
            outLines = getCenterLineRotate(mCenterLine, mOutLine.toPoint3D(), mDivideAngle);
            mVertexList.AddRange(outLines);
            for (int i = 0; i < outLines[0].Count; i++) {
                List<Point3D> plist = new List<Point3D>();
                for (int j = 0; j < outLines.Count; j++) {
                    plist.Add(outLines[j][i]);
                }
                plist.Add(outLines[0][i]);
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
            while (ang < mEa) {
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
        public override void translate(Point3D v, bool outline = false)
        {
            mCenterLine.translate(v);
            mOutLine.translate(v);
            if (!outline) {
                createSurfaceData();
                createVertexData();
            }
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">表示面</param>
        public override void rotate(Point3D cp, double ang, FACE3D face, bool outline = false)
        {
            mCenterLine.rotate(cp, ang, face);
            mOutLine.rotate(cp, ang, face);
            if (!outline) {
                createSurfaceData();
                createVertexData();
            }
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public override void offset(Point3D sp, Point3D ep, FACE3D face, bool outline = false)
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
                "DivideAngle", mDivideAngle.ToString(),
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
                    } else if (list[i] == "DivideAngle") {
                        mDivideAngle = double.TryParse(list[++i], out val) ? val : 0;
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
                System.Diagnostics.Debug.WriteLine(e.ToString());
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
            buf += $"CenterLine Sp {mCenterLine.mSp.ToString(form)} V {mCenterLine.mV.ToString(form)}";
            buf += $" Size {mOutLine.mPolyline.Count} OutLine ";
            for (int i = 0; i < mOutLine.mPolyline.Count; i++) {
                buf += "," + mOutLine.mPolyline[i].ToString(form);
            }
            return buf;
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        public override Primitive toCopy()
        {
            RevolutionPrimitive revolusion = new RevolutionPrimitive();
            revolusion.copyProperty(this, true);
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
        public override Polyline3D getVertexList()
        {
            return new Polyline3D();
        }

        /// <summary>
        /// ダミー
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="divideNo"></param>
        /// <param name="face"></param>
        /// <returns></returns>
        public override PointD nearPoint(PointD pos, int divideNo, FACE3D face)
        {
            return null;
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
        public double mDivideAngle = Math.PI / 9;
        public bool mClose = true;

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
        public SweepPrimitive(Polyline3D outline1, Polyline3D outline2, Brush color, bool close = true, FACE3D face = FACE3D.XY)
        {
            mPrimitiveId = PrimitiveId.Sweep;
            mPrimitiveFace = face;
            mLineColor = color;
            mFaceColors[0] = color;
            mOutLine1 = outline1.toCopy();
            mOutLine2 = outline2.toCopy();
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
            outLines = rotateOutlines(mOutLine1, mOutLine2, mDivideAngle);
            mVertexList.AddRange(outLines);
            for (int j = 0; j < outLines[0].Count; j++) {
                List<Point3D> plist = new List<Point3D>();
                for (int i = 0; i < outLines.Count; i++) {
                    plist.Add(outLines[i][j].toCopy());
                }
                plist.Add(outLines[0][j].toCopy());
                mVertexList.Add(plist);
            }
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
            (Point3D cp1, Point3D sp) = getStartCenter(l1, l2);
            l1.reverse();
            l2.reverse();
            (Point3D cp2, Point3D ep) = getStartCenter(l1, l2);
            l1.reverse();
            l2.reverse();
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
        public override void translate(Point3D v, bool outline = false)
        {
            mOutLine1.translate(v);
            mOutLine2.translate(v);
            if (!outline) {
                createSurfaceData();
                createVertexData();
            }
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">表示面</param>
        public override void rotate(Point3D cp, double ang, FACE3D face, bool outline = false)
        {
            mOutLine1.rotate(cp, ang, face);
            mOutLine2.rotate(cp, ang, face);
            if (!outline) {
                createSurfaceData();
                createVertexData();
            }
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public override void offset(Point3D sp, Point3D ep, FACE3D face, bool outline = false)
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
                "DivideAngle", mDivideAngle.ToString(),
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
                    } else if (list[i] == "DivideAngle") {
                        mDivideAngle = double.TryParse(list[++i], out val) ? val : 0;
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
                System.Diagnostics.Debug.WriteLine(e.ToString());
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
            buf += $"OutLine1 Cp {mOutLine1.mCp.ToString(form)} U {mOutLine1.mU.ToString(form)} V {mOutLine1.mV.ToString(form)}";
            buf += $" size {mOutLine1.mPolyline.Count}  OutLine1 ";
            for (int i = 0; i < mOutLine1.mPolyline.Count; i++) {
                buf += "," + mOutLine1.mPolyline[i].ToString(form);
            }
            buf += $"OutLine2 Cp {mOutLine2.mCp.ToString(form)} U {mOutLine2.mU.ToString(form)} V {mOutLine2.mV.ToString(form)}";
            buf += $" size {mOutLine2.mPolyline.Count}  OutLine2 ";
            for (int i = 0; i < mOutLine2.mPolyline.Count; i++) {
                buf += "," + mOutLine2.mPolyline[i].ToString(form);
            }
            return buf;
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        public override Primitive toCopy()
        {
            SweepPrimitive revolusion = new SweepPrimitive();
            revolusion.copyProperty(this, true);
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
        public override Polyline3D getVertexList()
        {
            return new Polyline3D();
        }

        /// <summary>
        /// ダミー
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="divideNo"></param>
        /// <param name="face"></param>
        /// <returns></returns>
        public override PointD nearPoint(PointD pos, int divideNo, FACE3D face)
        {
            return null;
        }
    }
}

