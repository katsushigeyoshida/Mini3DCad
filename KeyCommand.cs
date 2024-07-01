using CoreLib;

namespace Mini3DCad
{
    /// <summary>
    /// キー入力コマンド実行
    /// 
    /// 対応パラメータ
    ///  x〇y〇       絶対座標
    ///  dx〇,dy〇    相対座標
    ///  p〇          要素番号
    ///  r〇          半径
    ///  sa〇         開始角
    ///  ea〇         修了角
    ///  "〇〇"       文字列
    ///  〇〇         数値(IsDogit,-)
    /// </summary>
    public class KeyCommand
    {
        private List<string> mMainCmd = new List<string>() {
            "point", "line", "rect", "polyline", "polygon", "arc", "circle", "text",
            "translate", "rotate", "mirror", "trim", "stretch", "copy", "scaling",
            "color", "linetype", "thickness", "pointtype", "pointsize", "textsize", "ha", "va"
        };

        private List<Point3D> mPoints = new List<Point3D>();
        private List<int> mPickEnt = new List<int>();
        private double mRadius = 0;
        private double mSa = 0;
        private double mEa = Math.PI * 2;
        private double mValue = 0;
        private string mValString = "";
        private int mCommandNo = -1;
        private FACE3D mFace = FACE3D.XY;

        public List<string> mKeyCommandList = new();                //  キー入力コマンドの履歴
        public string mTextString = "";                             //  文字列データ
        public string mKeyommandPath = "KeyCommand.csv";            //  キーコマンド履歴ファイルパス
        public int mMaxKKeyCommand = 100;                           //  保存コマンド数
        public DataManage mDataManage;

        private YCalc ycalc = new YCalc();
        private YLib ylib = new YLib();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataManage"></param>
        public KeyCommand(DataManage dataManage)
        {
            mDataManage = dataManage;
            loadFile(mKeyommandPath);
        }

        /// <summary>
        /// コマンドの実行
        /// </summary>
        /// <param name="command">コマンド文字列</param>
        /// <param name="face">2D平面</param>
        /// <returns></returns>
        public bool execCommand(string command, FACE3D face)
        {
            if (command.Length == 0)
                return false;
            mFace = face;
            try {
                getCommandParameter(command);
                if (mMainCmd.Count < mCommandNo)
                    return false;
                switch (mMainCmd[mCommandNo]) {
                    case "point":
                        if (0 < mPoints.Count)
                            mDataManage.addPoint(mPoints[0]);
                        break;
                    case "line":
                        if (1 < mPoints.Count)
                            mDataManage.addLine(mPoints[0], mPoints[1]);
                        break;
                    case "rect":
                        if (1 < mPoints.Count)
                            mDataManage.addPolygon(rect2Plist(mPoints[0], mPoints[1]));
                        break;
                    case "circle":
                        if (1 < mPoints.Count)
                            mDataManage.addCircle(point2Circle(mPoints[0], mPoints[1]));
                        else if (mPoints.Count == 1 && 0 < mRadius)
                            mDataManage.addArc(point2Circle(mPoints[0], mRadius));
                        break;
                    case "arc":
                        if (2 < mPoints.Count)
                            mDataManage.addArc(new Arc3D(mPoints[0], mPoints[1], mPoints[2]));
                        break;
                    case "polyline":
                        if (1 < mPoints.Count)
                            mDataManage.addPolyline(mPoints);
                        break;
                    case "polygon":
                        if (2 < mPoints.Count)
                            mDataManage.addPolygon(mPoints);
                        break;
                    default:
                        return false;
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine($"execCommand: {e.Message}");
                return false;
            }
            mDataManage.updateData();
            return true;
        }

        /// <summary>
        /// コマンド文字列からコマンドやパラメータ値を求める
        /// </summary>
        /// <param name="command"></param>
        private void getCommandParameter(string command)
        {
            mCommandNo = -1;
            mRadius = 0;
            mSa = 0;
            mEa = 0;
            mValue = 0;
            mValString = "";
            mPickEnt.Clear();
            mPoints.Clear();
            List<string> cmd = commandSplit(command);
            for (int i = 0; i < cmd.Count; i++) {
                if (0 > cmd[i].IndexOf("\""))
                    cmd[i] = cmd[i].ToLower();
                if (mCommandNo < 0) {
                    mCommandNo = mMainCmd.FindIndex(p => 0 <= p.IndexOf(cmd[i]));
                    continue;
                }
                if (0 == cmd[i].IndexOf("x") || 0 == cmd[i].IndexOf("y") || 0 == cmd[i].IndexOf("z") ||
                    0 == cmd[i].IndexOf("dx") || 0 == cmd[i].IndexOf("dy") || 0 == cmd[i].IndexOf("dz")) {
                    //  座標/相対座標
                    Point3D dp = getPoint(cmd[i],
                        mPoints.Count < 1 ? new Point3D(0, 0, 0) : mPoints[mPoints.Count - 1]);
                    if (!dp.isNaN())
                        mPoints.Add(dp);
                } else if (0 == cmd[i].IndexOf("p")) {
                    //  要素番号
                    mPickEnt.Add(getIntPara(cmd[i], "p"));
                } else if (0 == cmd[i].IndexOf("r")) {
                    //  半径
                    mRadius = getPara(cmd[i], "r");
                } else if (0 == cmd[i].IndexOf("sa")) {
                    //  始角
                    mSa = getPara(cmd[i], "sa");
                } else if (0 == cmd[i].IndexOf("ea")) {
                    //  終角
                    mEa = getPara(cmd[i], "ea");
                } else if (char.IsDigit(cmd[i][0]) || cmd[i][0] == '-') {
                    //  数値
                    mValue = ylib.string2double(cmd[i]);
                } else if (cmd[i][0] == '"') {
                    //  文字列
                    mTextString = cmd[i].Trim('"');
                } else {
                    //  その他の文字列
                    mValString = cmd[i];
                }
            }
        }

        /// <summary>
        /// 円弧データに返還
        /// </summary>
        /// <param name="cp">中心座標</param>
        /// <param name="ep">円周上の座標</param>
        /// <returns>円弧</returns>
        private Arc3D point2Circle(Point3D cp, Point3D ep)
        {
            return new Arc3D(cp, cp.length(ep), mFace);
        }

        /// <summary>
        /// 円データに変換
        /// </summary>
        /// <param name="cp">中心座標</param>
        /// <param name="r">半径</param>
        /// <returns>円弧</returns>
        private Arc3D point2Circle(Point3D cp, double r)
        {
            return new Arc3D(cp, r, mFace);
        }

        /// <summary>
        /// rectからPolylineデータに変換
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <returns>座標リスト</returns>
        private List<Point3D> rect2Plist(Point3D sp, Point3D ep)
        {
            List<Point3D> plist = new List<Point3D>();
            plist.Add(sp);
            if (mFace == FACE3D.FRONT)
                plist.Add(new Point3D(sp.x, ep.y, sp.z));
            else if (mFace == FACE3D.TOP)
                plist.Add(new Point3D(sp.x, sp.y, ep.z));
            else if (mFace == FACE3D.RIGHT)
                plist.Add(new Point3D(sp.x, sp.y, ep.z));
            plist.Add(ep);
            if (mFace == FACE3D.FRONT)
                plist.Add(new Point3D(ep.x, sp.y, sp.z));
            else if (mFace == FACE3D.TOP)
                plist.Add(new Point3D(ep.x, sp.y, sp.z));
            else if (mFace == FACE3D.RIGHT)
                plist.Add(new Point3D(sp.x, ep.y, sp.z));
            return plist;
        }

        /// <summary>
        /// パラメータ文字列からPointDの値に変換(計算式可)
        /// </summary>
        /// <param name="xy">パラメータ文字列</param>
        /// <param name="a">パラメータ名</param>
        /// <param name="b">パラメータ名</param>
        /// <returns>パラメータ値</returns>
        private Point3D getPoint(string xyz, string a = "x", string b = "y", string c = "z")
        {
            int xn = xyz.IndexOf(a);
            int yn = xyz.IndexOf(b);
            int zn = xyz.IndexOf(c);
            double x = ycalc.expression(xyz.Substring(xn + a.Length, yn - a.Length));
            double y = ycalc.expression(xyz.Substring(yn + b.Length, zn - b.Length));
            double z = ycalc.expression(xyz.Substring(zn + c.Length));
            return new Point3D(x, y, z);
        }

        /// <summary>
        /// パラメータ文字列(座標/相対座標)からPointDの値に変換(計算式可)
        /// </summary>
        /// <param name="xy">パラメータ文字列</param>
        /// <param name="prev">前座標</param>
        /// <returns>座標</returns>
        private Point3D getPoint(string xyz, Point3D prev)
        {
            Point3D p = new Point3D();
            bool zflag = false;
            string[] sep = { "x", "y", "z", "dx", "dy", "dz", ",", " " };
            List<string> list = ylib.splitString(xyz, sep);
            for (int i = 0; i < list.Count; i++) {
                if (list[i] == "x" && i + 1 < list.Count) {
                    p.x = ycalc.expression(list[++i]);
                } else if (list[i] == "y" && i + 1 < list.Count) {
                    p.y = ycalc.expression(list[++i]);
                } else if (list[i] == "z" && i + 1 < list.Count) {
                    p.z = ycalc.expression(list[++i]);
                    zflag = true;
                } else if (list[i] == "dx" && i + 1 < list.Count) {
                    p.x = ycalc.expression(list[++i]) + prev.x;
                } else if (list[i] == "dy" && i + 1 < list.Count) {
                    p.y = ycalc.expression(list[++i]) + prev.y;
                } else if (list[i] == "dz" && i + 1 < list.Count) {
                    p.z = ycalc.expression(list[++i]) + prev.z;
                    zflag = true;
                }
            }
            if (!zflag) {
                PointD pos = new PointD(p.x, p.y);
                p = new Point3D(pos, mFace);
            }

            return p;
        }

        /// <summary>
        /// パラメータ文字列から値に変換(計算式可)
        /// </summary>
        /// <param name="paraStr">パラメータ文字列</param>
        /// <param name="para">パラメータの種類</param>
        /// <returns>パラメータ値</returns>
        private double getPara(string paraStr, string para)
        {
            int rn = paraStr.IndexOf(para);
            double r = ycalc.expression(paraStr.Substring(rn + 1));
            return r;
        }

        /// <summary>
        /// パラメータ文字列から整数値に変換
        /// </summary>
        /// <param name="paraStr">パラメータ文字列</param>
        /// <param name="para">パラメータの種類</param>
        /// <returns>パラメータ値</returns>
        private int getIntPara(string paraStr, string para)
        {
            int pn = paraStr.IndexOf(para);
            int n = (int)ycalc.expression(paraStr.Substring(pn + 1));
            return n;
        }

        /// <summary>
        /// コマンド文字列をコマンドやパラメータなどに分解
        /// </summary>
        /// <param name="command">コマンド文字列</param>
        /// <returns>パラメータリスト</returns>
        private List<string> commandSplit(string command)
        {
            List<string> cmd = new List<string>();
            string buf = "";
            for (int i = 0; i < command.Length; i++) {
                if (command[i] == ' ' || command[i] == ',') {
                    if (0 < buf.Length) {
                        cmd.Add(buf);
                        buf = "";
                    }
                } else if (command[i] == '"') {
                    if (0 < buf.Length) {
                        cmd.Add(buf);
                        buf = "";
                    }
                    buf += command[i++];
                    do {
                        buf += command[i];
                    } while (i < command.Length - 1 && command[i++] != '"');
                    cmd.Add(buf);
                    buf = "";
                } else {
                    buf += command[i];
                }
            }
            if (0 < buf.Length) {
                cmd.Add(buf);
                buf = "";
            }
            return cmd;
        }

        /// <summary>
        /// コマンド履歴の登録の
        /// </summary>
        /// <param name="command">コマンド文字列</param>
        /// <returns>コマンドリスト</returns>
        public List<string> keyCommandList(string command)
        {
            int n = mKeyCommandList.IndexOf(command);
            if (0 <= n)
                mKeyCommandList.RemoveAt(n);
            mKeyCommandList.Insert(0, command);
            return mKeyCommandList;
        }

        /// <summary>
        /// キーコマンドをファイルに保存
        /// </summary>
        public void saveFile()
        {
            saveFile(mKeyommandPath);
        }

        /// <summary>
        /// キーコマンドをファイルに保存
        /// </summary>
        /// <param name="path"></param>
        public void saveFile(string path)
        {
            List<string[]> comlist = new List<string[]>();
            for (int i = 0; i < mKeyCommandList.Count; i++) {
                comlist.Add([mKeyCommandList[i]]);
            }
            ylib.saveCsvData(path, comlist);
        }

        /// <summary>
        /// キーコマンドをファイルを読み込む
        /// </summary>
        /// <param name="path"></param>
        public void loadFile(string path)
        {
            List<string[]> llist = ylib.loadCsvData(path);
            if (llist != null) {
                for (int i = 0; i < llist.Count && i < mMaxKKeyCommand; i++)
                    mKeyCommandList.Add(llist[i][0]);
            }
        }
    }
}
