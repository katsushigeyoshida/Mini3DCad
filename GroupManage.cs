using CoreLib;

namespace Mini3DCad
{
    public class GroupManage
    {
        public Dictionary<int, string> mGroupList;      //  グループ名リスト

        private YLib ylib = new YLib();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GroupManage() 
        {
            mGroupList = new Dictionary<int, string>();
        }

        /// <summary>
        /// グループNoを抽出する
        /// </summary>
        /// <param name="elementList">要素リスト</param>
        /// <param name="groupNo">グループNo</param>
        /// <returns>グループNoリスト</returns>
        public List<int> getGroupNoList(List<Element> elementList, int groupNo) {
            List<int> groupList = new List<int>();
            if (elementList != null) {
                for (int i = 0; i < elementList.Count; i++)
                    if (elementList[i].mGroup == groupNo)
                        groupList.Add(i);
            }
            return groupList;
        }

        /// <summary>
        /// グループ名のリスト化
        /// </summary>
        /// <returns>グループ名リスト</returns>
        public List<string> getGroupNameList()
        {
            List<string> list = new List<string>();
            foreach (var item in mGroupList)
                list.Add(item.Value);
            return list;
        }

        /// <summary>
        /// グループ名の追加
        /// </summary>
        /// <param name="name">グループ名</param>
        /// <returns>グループNo</returns>
        public int add(string name)
        {
            if (!mGroupList.ContainsValue(name)) {
                int n = 1;
                while (mGroupList.ContainsKey(n))
                    n++;
                mGroupList[n] = name;
                return n;
            } else
            return getGroupNo(name);
        }

        /// <summary>
        /// グループ名の取得
        /// </summary>
        /// <param name="n">グループNo</param>
        /// <returns>グループ名</returns>
        public string getGroupName(int n)
        {
            if (mGroupList.ContainsKey(n))
                return mGroupList[n];
            else
                return "";
        }

        /// <summary>
        /// グループ名からグループNoを取得(0はグループ名登録なし)
        /// </summary>
        /// <param name="name">グループ名</param>
        /// <returns>グループNo</returns>
        public int getGroupNo(string name)
        {
            if (mGroupList.ContainsValue(name)) {
                var val = mGroupList.FirstOrDefault(x => x.Value == name);
                return val.Key;
            } else
                return 0;
        }

        /// <summary>
        /// 使用されていないグループ名を削除する
        /// </summary>
        /// <param name="elementList"></param>
        public void squeeze(List<Element> elementList)
        {
            List<int> noList = new List<int>();
            for (int i = 0; i < elementList.Count; i++) {
                if (0 < elementList[i].mGroup) {
                    if (!noList.Contains(elementList[i].mGroup))
                        noList.Add(elementList[i].mGroup);
                }
            }
            foreach (var item in mGroupList) {
                if (!noList.Contains(item.Key) || item.Value == "")
                    mGroupList.Remove(item.Key);
            }
        }

        /// <summary>
        /// グループデータを文字列配列リストに変換
        /// </summary>
        /// <returns>文字列配列リスト</returns>
        public List<string[]> toDataList()
        {
            List<string[]> list = new List<string[]>();
            string[] buf = { "Group" };
            list.Add(buf);
            List<string> strings = new List<string> { "GroupList" };
            foreach (var item in mGroupList) {
                strings.Add(item.Key.ToString());
                strings.Add(item.Value);
            }
            list.Add(strings.ToArray());
            buf = new string[] { "GroupEnd" };
            list.Add(buf);
            return list;
        }

        /// <summary>
        /// グループデータの設定
        /// </summary>
        /// <param name="dataList">文字列配列リスト</param>
        /// <param name="sp">リスト開始位置</param>
        /// <returns>リスト終了位置</returns>
        public int setDataList(List<string[]> dataList, int sp)
        {
            while (sp < dataList.Count) {
                string[] buf = dataList[sp++];
                if (buf[0] == "Group") {
                } else if (buf[0] == "GroupList") {
                    mGroupList.Clear();
                    for (int i = 1; i < buf.Length; i += 2) {
                        int groupNo = ylib.intParse(buf[i]);
                        mGroupList.Add(groupNo, buf[i + 1]);
                    }
                } else if (buf[0] == "GroupEnd") {
                    break;
                }
            }
            return sp;
        }
    }
}
