using CoreLib;
using System.Globalization;

namespace Mini3DCad
{
    /// <summary>
    /// レイヤ管理クラス
    /// </summary>
    public class Layer
    {
        public int mLayerSize = 8;                      //  レイヤーbyteサイズ (size x 8)
        public byte[] mDispLayerBit;                    //  レイヤーBit
        public bool mLayerAll = true;                   //  全レイヤー表示
        public Dictionary<int, string> mLayerList;      //  レイヤー名リスト

        private YLib ylib = new YLib();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="size">bitサイズ</param>
        public Layer(int size)
        {
            mLayerSize = size;
            mDispLayerBit = new byte[mLayerSize / 8];
            bitOnAll();
            mLayerList = new Dictionary<int, string>();
            add("BaseLayer");
            add("SecondLayer");
        }

        /// <summary>
        /// レイヤデータをクリア
        /// </summary>
        public void clear()
        {
            mDispLayerBit = new byte[mLayerSize / 8];
            if (mLayerList == null)
                mLayerList = new Dictionary<int, string>();
            else
                mLayerList.Clear();
        }

        /// <summary>
        /// レイヤデータを文字列配列リストに変換
        /// </summary>
        /// <returns>文字列配列リスト</returns>
        public List<string[]> toDataList()
        {
            List<string[]> list = new List<string[]>();
            string[] buf = { "Layer" };
            list.Add(buf);
            buf = new string[] { "LayerSize", mLayerSize.ToString() };
            list.Add(buf);
            List<string> strings = new List<string> { "DispLayerBit" };
            for (int i = 0; i < mDispLayerBit.Length; i++) {
                strings.Add(mDispLayerBit[i].ToString("X2"));
            }
            list.Add(strings.ToArray());
            buf = new string[] { "LayerAll", mLayerAll.ToString() };
            list.Add(buf);
            strings = new List<string> { "LayerList" };
            foreach (var item in mLayerList) {
                strings.Add(item.Key.ToString());
                strings.Add(item.Value);
            }
            list.Add(strings.ToArray());
            buf =　new string[] { "LayerEnd" };
            list.Add(buf);
            return list;
        }

        /// <summary>
        /// レイヤデータの設定
        /// </summary>
        /// <param name="dataList">文字列配列リスト</param>
        /// <param name="sp">リスト開始位置</param>
        /// <returns>リスト終了位置</returns>
        public int setDataList(List<string[]> dataList, int sp)
        {
            while (sp < dataList.Count) {
                string[] buf = dataList[sp++];
                if (buf[0] == "Layer") {
                } else if (buf[0] == "LayerSize") {
                    int layerSize = ylib.intParse(buf[1]);
                } else if (buf[0] == "DispLayerBit") {
                    for (int i = 0; i < mDispLayerBit.Length && i < buf.Length - 1; i++) {
                        mDispLayerBit[i] = byte.Parse(buf[i + 1], NumberStyles.HexNumber);
                    }
                } else if (buf[0] == "LayerAll") {
                    mLayerAll = ylib.boolParse(buf[1]);
                } else if (buf[0] == "LayerList") {
                    mLayerList.Clear();
                    for (int i = 1; i < buf.Length; i += 2) {
                        int layerNo = ylib.intParse(buf[i]);
                        if (layerNo < mLayerSize)
                            mLayerList.Add(layerNo, buf[i + 1]);
                    }
                } else if (buf[0] == "LayerEnd") {
                    break;
                }
            }
            return sp;
        }

        /// <summary>
        /// レイヤビットから使用レイヤー名を取得
        /// </summary>
        /// <param name="bytes">レイヤBit</param>
        /// <returns>レイヤ名リスト</returns>
        public List<string> getLayerNameList(byte[] bytes)
        {
            List<string> list = new List<string>();
            foreach (var keyvalue in mLayerList) {
                int n = keyvalue.Key;
                if (getBitOn(bytes, n)) {
                    list.Add(keyvalue.Value);
                }
            }
            return list;
        }

        /// <summary>
        /// レイヤ名の追加
        /// </summary>
        /// <param name="name">レイヤ名</param>
        /// <returns>レイヤNo</returns>
        public int add(string name)
        {
            if (mLayerList.ContainsValue(name)) {
                var val = mLayerList.FirstOrDefault(x => x.Value == name);
                return val.Key;
            } else {
                for (int i = 0; i < mLayerSize * 8; i++) {
                    if (!mLayerList.ContainsKey(i)) {
                        mLayerList.Add(i, name);
                        bitOn(i);
                        return i;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// レイヤ名の変更
        /// </summary>
        /// <param name="srcName">変更前のレイヤ名</param>
        /// <param name="destName">変更後のレイヤ名</param>
        public void rename(string srcName, string destName)
        {
            int n = getLayerNo(srcName);
            if (0 <= n)
                mLayerList[n] = destName;
        }

        /// <summary>
        /// リストにないデータを削除する
        /// </summary>
        /// <param name="keyList">レイヤNoリスト</param>
        /// <returns>レイヤー数</returns>
        public int upDateList(int[] keyList)
        {
            Dictionary<int, string> layerList = new Dictionary<int, string>();
            Array.Fill<byte>(mDispLayerBit, 0);
            foreach (int n in keyList) {
                if (!layerList.ContainsKey(n)) {
                    layerList.Add(n, mLayerList[n]);
                    bitOn(n);
                }
            }
            mLayerList = layerList;
            return mLayerList.Count;
        }

        /// <summary>
        /// レイヤNoの有無
        /// </summary>
        /// <param name="n">レイヤNo</param>
        /// <returns>有無</returns>
        public bool containsKey(int n)
        {
            return mLayerList.ContainsKey(n);
        }

        /// <summary>
        /// レイヤNoからレイヤ名の取得
        /// </summary>
        /// <param name="n">レイヤNo</param>
        /// <returns>レイヤ名</returns>
        public string gatLayerName(int n)
        {
            if (mLayerList.ContainsKey(n))
                return mLayerList[n];
            else
                return "";
        }

        /// <summary>
        /// レイヤ名からレイヤNoの取得
        /// </summary>
        /// <param name="name">レイヤ名</param>
        /// <returns>レイヤNo</returns>
        public int getLayerNo(string name)
        {
            if (mLayerList.ContainsValue(name)) {
                var val = mLayerList.FirstOrDefault(x => x.Value == name);
                return val.Key;
            } else
                return -1;
        }

        /// <summary>
        /// 表示レイヤBitsまたは空レイヤのレイヤチェックリストを作成
        /// </summary>
        /// <param name="clear">空レイヤリスト</param>
        /// <returns></returns>
        public List<CheckBoxListItem> getLayerChkList(bool clear = false)
        {
            if (clear) {
                byte[] layerBit = new byte[mLayerSize / 8];
                return getLayerChkList(layerBit);
            } else
                return getLayerChkList(mDispLayerBit);
        }

        /// <summary>
        /// レイヤBitsからレイヤチェックリストを作成
        /// </summary>
        /// <param name="layerBits">レイヤBits</param>
        /// <returns>レイヤチェックリスト</returns>
        public List<CheckBoxListItem> getLayerChkList(byte[] layerBits)
        {
            List<CheckBoxListItem> chkList = new List<CheckBoxListItem>();
            foreach (KeyValuePair<int, string> item in mLayerList) {
                CheckBoxListItem chkItem = new CheckBoxListItem(getBitOn(layerBits, item.Key), item.Value);
                chkList.Add(chkItem);
            }
            chkList.Sort((a, b) => a.Text.CompareTo(b.Text));
            return chkList;
        }

        /// <summary>
        /// 表示レイヤリストの設定
        /// </summary>
        /// <param name="chkList">レイヤチェックリスト</param>
        public void setLayerChkList(List<CheckBoxListItem> chkList)
        {
            //  chkListにない項目削除
            foreach (var item in mLayerList) {
                if (chkList.FindIndex(p => p.Text == item.Value) < 0) {
                    mLayerList.Remove(item.Key);
                }
            }
            //  チェック状態をBitListに反映
            foreach (CheckBoxListItem chkItem in chkList) {
                int n = getLayerNo(chkItem.Text);
                //  アイテムがないときは追加
                if (n < 0)
                    n = add(chkItem.Text);
                if (chkItem.Checked)
                    bitOn(n);
                else
                    bitOff(n);
            }
        }

        /// <summary>
        /// レイヤBitに設定
        /// </summary>
        /// <param name="layerBits">レイヤBit</param>
        /// <param name="chkList">レイヤチェックリスト</param>
        /// <param name="replace">置換え</param>
        /// <returns>反映後のレイヤBits</returns>
        public byte[] setLayerChkList(byte[] layerBits, List<CheckBoxListItem> chkList, bool replace = true)
        {
            foreach (CheckBoxListItem chkItem in chkList) {
                int n = getLayerNo(chkItem.Text);
                if (n < 0)
                    n = add(chkItem.Text);
                if (chkItem.Checked)
                    bitOn(layerBits, n);
                else if (replace)
                    bitOff(layerBits, n);
            }
            return layerBits;
        }

        /// <summary>
        /// レイヤBitの置換え
        /// </summary>
        /// <param name="layerbit">LayerBit</param>
        /// <param name="replaceData">置き換えリスト</param>
        /// <returns>置換え後のLayerBit</returns>
        public byte[] replaceOn(byte[] layerbit, List<int[]> replaceData)
        {
            byte[] bytes = new byte[layerbit.Length];
            List<int> bitNoList = getBitOnNo(layerbit);
            for (int i = 0; i < bitNoList.Count; i++) {
                int n = replaceData.FindIndex(p => p[0] == bitNoList[i]);
                if (0 <= n)
                    bitOn(bytes, replaceData[n][1]);   
            }
            return bytes;
        }

        /// <summary>
        /// OnBit位置の取得
        /// </summary>
        /// <param name="layerbit">LayerBit</param>
        /// <returns>On位置リスト</returns>
        public List<int> getBitOnNo(byte[] layerbit)
        {
            List<int> bitNoList= new List<int>();
            for (int i = 0; i < layerbit.Length * 8; i++) {
                if (getBitOn(layerbit, i))
                    bitNoList.Add(i);
            }
            return bitNoList;
        }

        /// <summary>
        /// レイヤBit(bytes)が 0 かの確認
        /// </summary>
        /// <param name="bytes">レイヤBit</param>
        /// <returns></returns>
        public bool IsEmpty(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
                if (bytes[i] != 0)
                    return false;
            return true;
        }

        /// <summary>
        /// bit and でデータの有無
        /// </summary>
        /// <param name="bytes">レイヤBit</param>
        /// <returns></returns>
        public bool bitAnd(byte[] bytes)
        {
            for (int i = 0; i < mDispLayerBit.Length; i++) {
                byte b = (byte)(mDispLayerBit[i] & bytes[i]);
                if (b != 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 表示レイヤBitsで指定位置のBitがOnかの確認
        /// </summary>
        /// <param name="n">Bit位置</param>
        /// <returns>On</returns>
        public bool getBitOn(int n)
        {
            return getBitOn(mDispLayerBit, n);
        }

        /// <summary>
        /// Bitデータで指定位置のBitがOnかの確認
        /// </summary>
        /// <param name="layerBits">Bitデータ</param>
        /// <param name="n">Bit位置</param>
        /// <returns>On</returns>
        public bool getBitOn(byte[] layerBits, int n)
        {
            byte lb = layerBits[n / 8];
            byte b = 1;
            b <<= n % 8;
            return 0 != (lb & b);
        }

        /// <summary>
        /// すべての表示レイヤBitをOnにする
        /// </summary>
        /// <param name="off">0クリア</param>
        public void bitOnAll(bool off = false)
        {
            bitOnAll(mDispLayerBit, off);
        }

        /// <summary>
        /// すべてのBitをOn/Offにする
        /// </summary>
        /// <param name="bytes">Bitデータ</param>
        /// <param name="off">0クリア</param>
        public void bitOnAll(byte[] bytes, bool off = false)
        {
            for (int i = 0; i < bytes.Length; i++) {
                if (off)
                    bytes[i] = 0x00;
                else
                    bytes[i] = 0xff;
            }
        }

        /// <summary>
        /// 表示レイヤBitをBit位置指定でOnにする
        /// </summary>
        /// <param name="n">Bit位置</param>
        public void bitOn(int n)
        {
            bitOn(mDispLayerBit, n);
        }

        /// <summary>
        /// BitデータをBit位置指定でOnにする
        /// </summary>
        /// <param name="bytes">Bitデータ</param>
        /// <param name="n">Bit位置</param>
        public void bitOn(byte[] bytes, int n)
        {
            byte b = 1;
            b <<= n % 8;
            bytes[n / 8] |= b;
        }

        /// <summary>
        /// 表示レイヤBitをBit位置指定でOffにする
        /// </summary>
        /// <param name="n">Bit位置</param>
        public void bitOff(int n)
        {
            bitOff(mDispLayerBit, n);
        }

        /// <summary>
        /// BitデータをBit位置指定でOffにする
        /// </summary>
        /// <param name="bytes">Bitデータ</param>
        /// <param name="n">Bit位置</param>
        public void bitOff(byte[] bytes, int n)
        {
            byte b = 1;
            b <<= n % 8;
            bytes[n / 8] &= (byte)~b;
        }

        /// <summary>
        /// 16進文字列に変換
        /// </summary>
        /// <param name="start">開始位置</param>
        /// <param name="size">サイズ</param>
        /// <returns>16進文字列</returns>
        public string binary2HexString(int start = 0, int size = 0)
        {
            string buf = "";
            if (size == 0) size = mDispLayerBit.Length;
            for (int i = start; i < start + size && i < mDispLayerBit.Length; i++) {
                buf += string.Format("{0:X2} ", mDispLayerBit[i]);
            }
            return buf;
        }
    }
}
