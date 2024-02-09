using CoreLib;

namespace Mini3DCad
{
    public class Element
    {
        public string mName;                    //  エレメント名(任意)
        public Primitive mPrimitive;            //  プリミティブリスト
        public List<Surface> mSurfaceList;      //  サーフェスデータリスト(LINE/TRYANGLE)
        public bool mBothShading = true;        //  両面陰面判定の有無
        public bool mDisp3D = true;             //  3Dでの表示/非表示
        public bool mRemove = false;            //  削除フラグ
        public Box3D mArea;                     //  要素領域
        public int mOperationNo = -1;           //  操作位置
        public int mLinkNo = -1;

        private YLib ylib = new YLib();
        public Element()
        {
            mSurfaceList = new List<Surface>();
            mName = "要素名";
        }

        /// <summary>
        /// 全データクリア
        /// </summary>
        public void clear()
        {
            mSurfaceList = new List<Surface>();
        }

        /// <summary>
        /// 2D表示処理
        /// </summary>
        /// <param name="draw"></param>
        /// <param name="face">表示面</param>
        public void draw2D(YWorldDraw draw, FACE3D face)
        {
            if (mArea != null && !draw.mClipBox.outsideChk(mArea.toBox(face)))
                mPrimitive.draw2D(draw, face);
        }

        /// <summary>
        /// コピーの作成
        /// </summary>
        /// <returns></returns>
        public Element toCopy()
        {
            Element element = new Element();
            element.mName = mName;
            element.mPrimitive = mPrimitive.toCopy();
            element.mSurfaceList = mSurfaceList.ConvertAll(p => p.toCopy());
            element.mBothShading = mBothShading;
            element.mDisp3D = mDisp3D;
            element.mRemove = mRemove;
            element.mArea = mArea.toCopy();
            element.mOperationNo = mOperationNo;
            element.mLinkNo = mLinkNo;
            return element;
        }

        /// <summary>
        /// エレメント情報
        /// </summary>
        /// <returns>文字列</returns>
        public string propertyInfo()
        {
            string buf = $"名称: {mName} プリミティブ: {mPrimitive}";
            buf += $" 両面表示: {mBothShading} 3D表示: {mDisp3D}";
            return buf;
        }

        /// <summary>
        /// エレメントの簡易情報
        /// </summary>
        /// <returns>文字列</returns>
        public string getSummary()
        {
            return $"{mName} {mPrimitive} {mArea.ToString("F2")}";
        }

        /// <summary>
        /// Primitiveデータ情報の取得
        /// </summary>
        /// <returns>文字列</returns>
        public string dataInfo()
        {
            string buf = mPrimitive.propertyInfo();
            buf += "\n" + ylib.insertLinefeed(mPrimitive.dataInfo("F2"), ",", 100);
            buf += "\nVertexList";
            List<string> list = mPrimitive.vertexInfo();
            foreach (string s in list)
                buf += $"\n{ylib.insertLinefeed(s, ",", 100)}";
            return buf;
        }

        /// <summary>
        /// 3D VertexListをUPDATEする
        /// </summary>
        public void update3DData()
        {
            if (mPrimitive != null)
                mArea = mPrimitive.getArea();
        }

        /// <summary>
        /// Elementデータを文字列配列リストに変換
        /// </summary>
        /// <returns>文字列配列リスト</returns>
        public List<string[]> toDataList()
        {
            List<string[]> list = new List<string[]>();
            string[] buf = { "Element" };
            list.Add(buf);
            buf = new string[] { "Name", mName };
            list.Add(buf);
            list.Add(mPrimitive.toPropertyList());
            list.Add(mPrimitive.toDataList());
            buf = new string[] { "IsShading", mBothShading.ToString() };
            list.Add(buf);
            buf = new string[] { "Disp3D", mDisp3D.ToString() };
            list.Add(buf);
            buf = new string[] { "ElementEnd" };
            list.Add(buf);
            return list;
        }

        /// <summary>
        /// 文字列配列データを変換してElementデータに設定
        /// </summary>
        /// <param name="dataList">文字列配列リスト</param>
        /// <param name="sp">リスト開始位置</param>
        /// <returns>リスト終了位置</returns>
        public int setDataList(List<string[]> dataList, int sp)
        {
            while (sp < dataList.Count) {
                string[] buf = dataList[sp++];
                try {
                    if (buf[0] == "PrimitiveId") {
                        switch (buf[1]) {
                            case "Line":
                                LinePrimitive line = new LinePrimitive();
                                line.setPropertyList(buf);
                                buf = dataList[sp++];
                                line.setDataList(buf);
                                mPrimitive = line;
                                break;
                            case "Arc":
                                ArcPrimitive arc = new ArcPrimitive();
                                arc.setPropertyList(buf);
                                buf = dataList[sp++];
                                arc.setDataList(buf);
                                mPrimitive = arc;
                                break;
                            case "Polyline":
                                PolylinePrimitive polyline = new PolylinePrimitive();
                                polyline.setPropertyList(buf);
                                buf = dataList[sp++];
                                polyline.setDataList(buf);
                                mPrimitive = polyline;
                                break;
                            case "Polygon":
                                PolygonPrimitive polygon = new PolygonPrimitive();
                                polygon.setPropertyList(buf);
                                buf = dataList[sp++];
                                polygon.setDataList(buf);
                                mPrimitive = polygon;
                                break;
                            case "Extrusion":
                                ExtrusionPrimitive extrusion = new ExtrusionPrimitive();
                                extrusion.setPropertyList(buf);
                                buf = dataList[sp++];
                                extrusion.setDataList(buf);
                                mPrimitive = extrusion;
                                break;
                            case "Revolution":
                                RevolutionPrimitive revolution = new RevolutionPrimitive();
                                revolution.setPropertyList(buf);
                                buf = dataList[sp++];
                                revolution.setDataList(buf);
                                mPrimitive = revolution;
                                break;
                            default:
                                sp++;
                                break;
                        }
                        if (mPrimitive != null)
                            mPrimitive.createVertexList();
                    } else if (buf[0] == "Name") {
                        mName = buf[1];
                    } else if (buf[0] == "IsShading") {
                        mBothShading = ylib.boolParse(buf[1]);
                    } else if (buf[0] == "Disp3D") {
                        mDisp3D = ylib.boolParse(buf[1]);
                    } else if (buf[0] == "ElementEnd") {
                        break;
                    }
                } catch (Exception e) {
                    System.Diagnostics.Debug.WriteLine($"setDataList: {sp} : {e.Message}");
                }
            }
            if (mPrimitive != null)
                update3DData();
            return sp;
        }
    }
}
