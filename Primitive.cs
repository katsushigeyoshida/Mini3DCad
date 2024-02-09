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
        Line, Arc, Polyline, Polygon, WireCube,
        Cube, Cylinder, Sphere, Cone,
        Revolution, Extrusion, Sweep
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
        public Brush mLineColor = Brushes.Black;                                //  線の色
        public List<Brush> mFaceColors = new List<Brush>() { Brushes.Blue };    //  面の色
        public Brush mPickColor = Brushes.Red;                                  //  ピック時のカラー
        public bool mPick = false;                                              //  ピック状態
        public List<SurfaceData> mSurfaceDataList;                              //  3D座標データ


        public YLib ylib = new YLib();

        /// <summary>
        /// 3D座標(Surface)リストの作成
        /// </summary>
        public abstract void createVertexList();

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

        public abstract PointD nearPoint(PointD pos, int divideNo, FACE3D face);

        /// <summary>
        /// Surfaceデータの移動
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public void translateSurfacedata(Point3D v)
        {
            for (int i = 0; i < mSurfaceDataList.Count; i++) {
                mSurfaceDataList[i].translate(v);
            }
        }

        /// <summary>
        /// Surfaceデータの回転
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角度</param>
        /// <param name="face">2D平面</param>
        public void rotateSurfacedata(Point3D cp, double ang, FACE3D face)
        {
            for (int i = 0; i < mSurfaceDataList.Count; i++) {
                mSurfaceDataList[i].rotate(cp, ang, face);
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
            mFaceColors[0] = primitive.mFaceColors[0];
            mPickColor = primitive.mPickColor;
            mPick = primitive.mPick;
            if (dataList)
                mSurfaceDataList = primitive.mSurfaceDataList.ConvertAll(p => p.toCopy());
        }

        /// <summary>
        /// 2D 表示(XY/YZ/ZX)(ワイヤーフレームのみ表示)
        /// </summary>
        /// <param name="draw">グラフィック</param>
        /// <param name="face">表示面</param>
        public void draw2D(YWorldDraw draw, FACE3D face)
        {
            if (mPick)
                draw.mBrush = mPickColor;
            else
                draw.mBrush = mLineColor;
            draw.mThickness = mLineThickness;
            List<PointD> plist;
            for (int i = 0; i < mSurfaceDataList.Count; i++) {
                List<PointD> pplist = mSurfaceDataList[i].toPointDList(face);
                DRAWTYPE drawType = mSurfaceDataList[i].mDrawType;
                draw.mFillColor = null;
                switch (drawType) {
                    case DRAWTYPE.LINES:
                        for (int k = 0; k < pplist.Count - 1; k += 2)
                            draw.drawWLine(pplist[k], pplist[k + 1]);
                        break;
                    case DRAWTYPE.LINE_STRIP:
                        draw.drawWPolyline(pplist);
                        break;
                    case DRAWTYPE.LINE_LOOP:
                        pplist.Add(pplist[0]);
                        draw.drawWPolyline(pplist);
                        break;
                    case DRAWTYPE.TRIANGLES:
                        for (int k = 0; k < pplist.Count - 2; k += 3) {
                            plist = new List<PointD>() {
                                    pplist[k], pplist[k + 1], pplist[k + 2]
                                };
                            draw.drawWPolygon(plist, false);
                        }
                        break;
                    case DRAWTYPE.QUADS:
                        for (int k = 0; k < pplist.Count - 3; k += 4) {
                            plist = new List<PointD>() {
                                    pplist[k], pplist[k + 1], pplist[k + 2], pplist[k + 3]
                                };
                            draw.drawWPolygon(plist, false);
                        }
                        break;
                    case DRAWTYPE.TRIANGLE_STRIP:
                        for (int k = 0; k < pplist.Count - 2; k++) {
                            plist = new List<PointD>() {
                                    pplist[k], pplist[k + 1], pplist[k + 2]
                                };
                            draw.drawWPolygon(plist, false);
                        }
                        break;
                    case DRAWTYPE.QUAD_STRIP:
                        for (int k = 0; k < pplist.Count - 3; k += 2) {
                            plist = new List<PointD>() {
                                    pplist[k], pplist[k + 1], pplist[k + 3], pplist[k + 2]
                                };
                            draw.drawWPolygon(plist, false);
                        }
                        break;
                    case DRAWTYPE.TRIANGLE_FAN:
                        for (int k = 1; k < pplist.Count - 1; k++) {
                            plist = new List<PointD>() {
                                    pplist[0], pplist[k], pplist[k + 1]
                                };
                            draw.drawWPolygon(plist, false);
                        }
                        break;
                    default:
                        draw.drawWPolygon(pplist, false);
                        break;
                }
            }
        }

        /// <summary>
        /// Boxのピックの有無を調べる(2D)
        /// </summary>
        /// <param name="b">ピック領域</param>
        /// <param name="face">表示面</param>
        /// <returns>ピックの有無</returns>
        public bool pickChk(Box b, FACE3D face)
        {
            for (int i = 0; i < mSurfaceDataList.Count; i++) {
                List<List<PointD>> pplist = mSurfaceDataList.ConvertAll(p => p.toPointDList(face));
                foreach (var plist in pplist) {
                    if (mSurfaceDataList[i].mDrawType == DRAWTYPE.LINE_LOOP ||
                        mSurfaceDataList[i].mDrawType == DRAWTYPE.TRIANGLES ||
                        mSurfaceDataList[i].mDrawType == DRAWTYPE.QUADS ||
                        mSurfaceDataList[i].mDrawType == DRAWTYPE.POLYGON)
                        plist.Add(plist[0]);
                    if (0 < b.intersection(plist, false, true).Count || b.insideChk(plist))
                        return true;
                }
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
            buf += $" LineColor:{ylib.getBrushName(mLineColor)}, FaceColor:{ylib.getBrushName(mFaceColors[0])}, Thickness:{mLineThickness}";
            return buf;
        }

        /// <summary>
        /// 座標情報
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
            createVertexList();
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
            createVertexList();
        }

        /// <summary>
        /// 3D座標(Surface)リストの作成
        /// </summary>
        /// <returns>座標リスト</returns>
        public override void createVertexList()
        {
            mSurfaceDataList = new List<SurfaceData>();
            SurfaceData surfaceData = new SurfaceData();
            surfaceData.mVertexList = mLine.toPoint3D();
            surfaceData.mDrawType = DRAWTYPE.LINE_STRIP;
            surfaceData.mFaceColor = mLineColor;
            mSurfaceDataList.Add(surfaceData);
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public override void translate(Point3D v)
        {
            mLine.translate(v);
            createVertexList();
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
            createVertexList();
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
        public ArcPrimitive(PointD cp, double r, FACE3D face = FACE3D.XY)
        {
            mPrimitiveId = PrimitiveId.Arc;
            mPrimitiveFace = face;
            mArc = new Arc3D(new Point3D(cp, face), r, face);
            createVertexList();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="arc">2D円弧</param>
        /// <param name="face">2D平面</param>
        public ArcPrimitive(ArcD arc, FACE3D face = FACE3D.XY)
        {
            mPrimitiveId = PrimitiveId.Arc;
            mPrimitiveFace = face;
            mArc = new Arc3D(arc, face);
            createVertexList();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="arc">円弧</param>
        /// <param name="face">2D平面</param>
        public ArcPrimitive(Arc3D arc, FACE3D face = FACE3D.XY)
        {
            mPrimitiveId = PrimitiveId.Arc;
            mPrimitiveFace = face;
            mArc = arc.toCopy();
            createVertexList();
        }

        /// <summary>
        /// 3D座標(Surface)リストの作成
        /// </summary>
        /// <returns>座標リスト</returns>
        public override void createVertexList()
        {
            mSurfaceDataList = new List<SurfaceData>();
            SurfaceData surfaceData = new SurfaceData();
            surfaceData.mVertexList = mArc.toPoint3D(mDivideAngle);
            surfaceData.mDrawType = DRAWTYPE.LINE_STRIP;
            surfaceData.mFaceColor = mLineColor;
            mSurfaceDataList.Add(surfaceData);
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public override void translate(Point3D v)
        {
            mArc.translate(v);
            createVertexList();
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
            createVertexList();
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
            createVertexList();
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
            createVertexList();
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
            createVertexList();
        }

        /// <summary>
        /// 3D座標リストの作成
        /// </summary>
        public override void createVertexList()
        {
            mSurfaceDataList = new List<SurfaceData>();
            SurfaceData surfaceData = new SurfaceData();
            surfaceData.mVertexList = mPolyline.toPoint3D();
            surfaceData.mDrawType = DRAWTYPE.LINE_STRIP;
            surfaceData.mFaceColor = mLineColor;
            mSurfaceDataList.Add(surfaceData);
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public override void translate(Point3D v)
        {
            mPolyline.translate(v);
            createVertexList();
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
            createVertexList();
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
            createVertexList();
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
            createVertexList();
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
            createVertexList();
        }

        /// <summary>
        /// 3D座標リストの作成(三角形の集合)
        /// </summary>
        public override void createVertexList()
        {
            mSurfaceDataList = new List<SurfaceData>();
            List<Point3D> triangles = mPolygon.cnvTriangles();
            if (triangles.Count < 3)
                return;
            SurfaceData surfaceData = new SurfaceData();
            surfaceData.mVertexList = triangles;
            surfaceData.mDrawType = DRAWTYPE.TRIANGLES;
            surfaceData.mFaceColor = mFaceColors[0];
            mSurfaceDataList.Add(surfaceData);
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public override void translate(Point3D v)
        {
            mPolygon.translate(v);
            createVertexList();
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
            createVertexList();
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
            createVertexList();
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
            createVertexList();
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
            createVertexList();
        }

        /// <summary>
        /// 3D座標リストの作成
        /// </summary>
        public override void createVertexList()
        {
            mSurfaceDataList = new List<SurfaceData>();
            SurfaceData surfaceData;
            //  1面
            surfaceData = new SurfaceData();
            surfaceData.mVertexList = mPolygon.cnvTriangles();
            Point3D v0 = surfaceData.mVertexList[0].getNormal(surfaceData.mVertexList[1], surfaceData.mVertexList[2]);
            if (v0.angle(mVector) < Math.PI / 2)
                surfaceData.mVertexList.Reverse();
            surfaceData.mDrawType = DRAWTYPE.TRIANGLES;
            surfaceData.mFaceColor = mFaceColors[0];
            mSurfaceDataList.Add(surfaceData);
            //  2面
            surfaceData = new SurfaceData();
            surfaceData.mVertexList = mSurfaceDataList[0].mVertexList.ConvertAll(p => p.toCopy());
            surfaceData.mVertexList.ForEach(p => p.translate(mVector));
            if (v0.angle(mVector) > Math.PI / 2)
                surfaceData.mVertexList.Reverse();
            surfaceData.mDrawType = DRAWTYPE.TRIANGLES;
            surfaceData.mFaceColor = mFaceColors[0];
            mSurfaceDataList.Add(surfaceData);
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
            if (!mClose) {
                mSurfaceDataList.RemoveAt(1);
                mSurfaceDataList.RemoveAt(0);
            }
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public override void translate(Point3D v)
        {
            mPolygon.translate(v);
            translateSurfacedata(v);
            createVertexList();
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
            rotateSurfacedata(cp, ang, face);
            createVertexList();
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
        public Line3D mCenterLine = new Line3D();
        public List<Point3D> mOutLine = new List<Point3D>();
        public double mSa = 0;
        public double mEa = Math.PI * 2;
        public int mDivideNo = 20;
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
        public RevolutionPrimitive(Line3D centerLine, List<Point3D> outLine, Brush color, bool close = true, FACE3D face = FACE3D.XY)
        {
            mPrimitiveId = PrimitiveId.Revolution;
            mPrimitiveFace = face;
            mLineColor = color;
            mFaceColors[0] = color;
            mCenterLine = centerLine.toCopy();
            mOutLine = outLine.ConvertAll(p => p.toCopy());
            mClose = close;
            createVertexList();
        }

        /// <summary>
        /// 3D座標リストの作成
        /// </summary>
        public override void createVertexList()
        {
            mSurfaceDataList = new List<SurfaceData>();
            SurfaceData surfaceData;
            //  回転座標作成
            List<List<Point3D>> outLines = new List<List<Point3D>>();
            Point3D cp = mCenterLine.mSp.toCopy();
            List<Point3D> outline = mOutLine.ConvertAll(p => p.toCopy());
            cp.inverse();
            outline.ForEach(p => p.add(cp));
            cp.inverse();
            double ang = mSa;
            double dang = Math.PI * 2 / mDivideNo;
            while (ang < mEa) {
                List<Point3D> plist = outline.ConvertAll(p => p.toCopy());
                plist.ForEach(p => p.rotate(mCenterLine.mV, ang));
                plist.ForEach(p => p.add(cp));
                outLines.Add(plist);
                ang += dang;
            }
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
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public override void translate(Point3D v)
        {
            mCenterLine.translate(v);
            mOutLine.ForEach(p => p.add(v));
            translateSurfacedata(v);
            //createVertexList();
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
            mOutLine.ForEach(p => p.rotate(cp, ang, face));
            rotateSurfacedata(cp, ang, face);
            //createVertexList();
        }

        /// <summary>
        /// 固有データを文字列配列に変換
        /// </summary>
        /// <returns>文字列配列</returns>
        public override string[] toDataList()
        {
            List<string> dataList = new List<string>() {
                "RevolutionData",
                "CenterLineSp", mCenterLine.mSp.x.ToString(), mCenterLine.mSp.y.ToString(), mCenterLine.mSp.z.ToString(),
                "CenterLineVector", mCenterLine.mV.x.ToString(), mCenterLine.mV.y.ToString(), mCenterLine.mV.z.ToString(),
                "StartAngle", mSa.ToString(),
                "EndAngle", mEa.ToString(),
                "DivideNo", mDivideNo.ToString(),
                "Close", mClose.ToString(),
                "OutLineSize", mOutLine.Count.ToString(),
                "OutLine"
            };
            for (int i = 0; i < mOutLine.Count; i++) {
                dataList.Add(mOutLine[i].x.ToString());
                dataList.Add(mOutLine[i].y.ToString());
                dataList.Add(mOutLine[i].z.ToString());
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
                mOutLine = new List<Point3D>();
                int ival;
                double val;
                bool bval;
                int i = 1;
                int count;
                while (i < list.Length) {
                    if (list[i] == "CenterLineSp") {
                        Point3D p = new Point3D();
                        p.x = double.TryParse(list[++i], out val) ? val : 0;
                        p.y = double.TryParse(list[++i], out val) ? val : 0;
                        p.z = double.TryParse(list[++i], out val) ? val : 0;
                        mCenterLine.mSp = p;
                    } else if (list[i] == "CenterLineVector") {
                        Point3D p = new Point3D();
                        p.x = double.TryParse(list[++i], out val) ? val : 0;
                        p.y = double.TryParse(list[++i], out val) ? val : 0;
                        p.z = double.TryParse(list[++i], out val) ? val : 0;
                        mCenterLine.mV = p;
                    } else if (list[i] == "StartAngle") {
                        mSa = double.TryParse(list[++i], out val) ? val : 0;
                    } else if (list[i] == "EndAngle") {
                        mEa = double.TryParse(list[++i], out val) ? val : 0;
                    } else if (list[i] == "DivideNo") {
                        mDivideNo = int.TryParse(list[++i], out ival) ? ival : 0;
                    } else if (list[i] == "Close") {
                        mClose = bool.TryParse(list[++i], out bval) ? bval : true;
                    } else if (list[i] == "OutLineSize") {
                        count = int.TryParse(list[++i], out ival) ? ival : 0;
                    } else if (list[i] == "OutLine") {
                    } else {
                        Point3D p = new Point3D();
                        p.x = double.TryParse(list[i], out val) ? val : 0;
                        p.y = double.TryParse(list[++i], out val) ? val : 0;
                        p.z = double.TryParse(list[++i], out val) ? val : 0;
                        mOutLine.Add(p);
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
            buf += "CenterLine " + mCenterLine.ToString(form);
            buf += " StartAngle " + mSa.ToString(form);
            buf += " EndAngle " + mEa.ToString(form);
            buf += " DivideNo " + mDivideNo.ToString(form);
            buf += " Close " + mClose.ToString();
            buf += " OutLine ";
            for (int i = 0; i < mOutLine.Count; i++) {
                buf += "," + mOutLine[i].ToString(form);
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
            revolusion.mCenterLine = mCenterLine.toCopy();
            revolusion.mOutLine = mOutLine.ConvertAll(p => p.toCopy());
            revolusion.mDivideNo = mDivideNo;
            revolusion.mClose = mClose;
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

