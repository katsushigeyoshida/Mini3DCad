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
        Extrusion, Blend, BlendPolyline, Revolution, Sweep
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
        public bool mWireFrame = false;                                         //  3Dワイヤーフレーム表示
        public double mDivideAngle = Math.PI / 15;                              //  円弧の線分変換角度
        public bool mEdgeDisp = true;                                           //  単面表示(3Dデータのみ)
        public bool mOutlineDisp = false;                                       //  3D要素2Dデータ表示
        public List<Brush> mOutlineColors = new List<Brush>() { Brushes.Blue }; //  3D要素2Dデータ表示色
        public List<int> mOutlineType = new List<int>() { 0 };                  //  3D要素2Dデータ表示線種

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
        public abstract void translate(Point3D v, PointD pickPos, FACE3D face);

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">操作面</param>
        public abstract void rotate(Point3D cp, double ang, PointD pickPos, FACE3D face);

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">操作面</param>
        public abstract void offset(Point3D sp, Point3D ep, PointD pickPos, FACE3D face);

        /// <summary>
        /// ミラー
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D操作面</param>
        public abstract void mirror(Point3D sp, Point3D ep, PointD pickPos, FACE3D face);

        /// <summary>
        /// トリム
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public abstract void trim(Point3D sp, Point3D ep, PointD pickPos, FACE3D face);

        /// <summary>
        /// 拡大縮小
        /// </summary>
        /// <param name="cp">拡大中心</param>
        /// <param name="scale">倍率</param>
        /// <param name="face">2D平面</param>
        public abstract void scale(Point3D cp, double scale, PointD pickPos, FACE3D face);

        /// <summary>
        /// ストレッチ
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="arc">円弧変形</param>
        /// <param name="face">"D平面</param>
        public abstract void stretch(Point3D vec, bool arc, PointD pickPos, FACE3D face);


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
        /// 属性をコピーする
        /// </summary>
        /// <param name="primitive">Primitive</param>
        /// <param name="dataList">データリスト</param>
        /// <param name="id">Primitive ID</param>
        public void copyProperty(Primitive primitive, bool dataList = false, bool id = false)
        {
            if (id)
                mPrimitiveId = primitive.mPrimitiveId;
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
            mWireFrame = primitive.mWireFrame;
            mEdgeDisp = primitive.mEdgeDisp;
            mOutlineDisp = primitive.mOutlineDisp;
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
                if (!mPick && mOutlineDisp) {
                    draw.mBrush = mOutlineColors[i % mOutlineColors.Count];
                    draw.mLineType = mOutlineType[i % mOutlineType.Count];
                }
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
        public Point3D nearPoint(PointD pos, int divideNo, FACE3D face)
        {
            //  2D平面の線分リストに変換
            List<LineD> llist = new List<LineD>();
            for (int i = 0; i < mVertexList.Count; i++) {
                for (int j = 0; j < mVertexList[i].Count - 1; j++) {
                    LineD l = new LineD(mVertexList[i][j].toPoint(face), mVertexList[i][j + 1].toPoint(face));
                    llist.Add(l);
                }
            }
            //  指定座標に最も近い線分との距離と位置を求める
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
            if (n < 0) return null;

            (int ii, int jj) = getVertexListNo(n);
            if (ii < 0 || jj < 0) return null;
            //  3D線分上の交点を求める
            Line3D line = new Line3D(mVertexList[ii][jj], mVertexList[ii][jj + 1]);
            Point3D cp = line.intersection(pos, face);
            if (divideNo <= 0)
                return cp;

            List<Point3D> plist = line.divide(divideNo);
            return plist.MinBy(p => p.length(cp));
        }

        /// <summary>
        /// 線分の位置からVertexListの位置(2次元)を求める
        /// </summary>
        /// <param name="n">線分の位置</param>
        /// <returns>vertexの位置(i,j)</returns>
        private (int, int) getVertexListNo(int n)
        {
            int nn = 0;
            for (int i = 0; i < mVertexList.Count; i++) {
                for (int j = 0; j < mVertexList[i].Count - 1; j++) {
                    if (nn == n)
                        return (i, j);
                    nn++;
                }
            }
            return (-1, -1);
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
        /// ポリゴンから３角形の座標リストを作成
        /// </summary>
        /// <param name="polygon">ポリゴン</param>
        /// <returns>３角形座標リスト</returns>
        public List<List<Point3D>> getVertexList(Polygon3D polygon)
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
        /// PolygonのSurfaceデータの作成
        /// </summary>
        /// <param name="polygon">Polygonデータ</param>
        /// <param name="vector">表面の向き</param>
        /// <param name="divideAngle">円弧分割角度</param>
        /// <param name="faceColor">表面色</param>
        /// <returns>SurfaceData</returns>
        public SurfaceData createSurfaceData(Polygon3D polygon, Point3D vector, double divideAngle, Brush faceColor)
        {
            SurfaceData surfaceData = new SurfaceData();
            (surfaceData.mVertexList, bool reverse) = polygon.cnvTriangles(divideAngle);
            Point3D v0 = surfaceData.mVertexList[0].getNormal(surfaceData.mVertexList[1], surfaceData.mVertexList[2]);
            if (v0.angle(vector) < Math.PI / 2) {
                surfaceData.mVertexList.Reverse();
            }
            surfaceData.mDrawType = DRAWTYPE.TRIANGLES;
            surfaceData.mFaceColor = faceColor;
            surfaceData.reverse(mReverse);
            return surfaceData;
        }

        /// <summary>
        /// プロパティデータを文字列リストに変換
        /// </summary>
        /// <returns>文字列リスト</returns>
        public string[] toPropertyList()
        {
            List<string> dataList = new List<string> {
                "PrimitiveId",      mPrimitiveId.ToString(),
                "LineColor",        ylib.getBrushName(mLineColor),
                "LineThickness",    mLineThickness.ToString(),
                "LineType",         mLineType.ToString(),
                "Close",            mEdgeDisp.ToString(),
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
                } else if (list[i] == "LineColor") {
                    mLineColor = ylib.getBrsh(list[++i]);
                } else if (list[i] == "LineThickness") {
                    mLineThickness = double.TryParse(list[++i], out val) ? val : 1;
                } else if (list[i] == "LineType") {
                    mLineType = int.TryParse(list[++i], out ival) ? ival : 0;
                } else if (list[i] == "Close") {
                    mEdgeDisp = bool.TryParse(list[++i], out bval) ? bval : false;
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
}

