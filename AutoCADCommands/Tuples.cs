using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCADCommands
{
#if net20
    /// <summary>
    /// 二元组
    /// </summary>
    /// <typeparam name="T1">元素1类型</typeparam>
    /// <typeparam name="T2">元素2类型</typeparam>
    public class Tuple<T1, T2>
    {
        private T1 _item1;
        private T2 _item2;

        public T1 Item1 { get { return _item1; } }
        public T2 Item2 { get { return _item2; } }

        public Tuple(T1 item1, T2 item2)
        {
            _item1 = item1;
            _item2 = item2;
        }
    }

    /// <summary>
    /// 三元组
    /// </summary>
    /// <typeparam name="T1">元素1类型</typeparam>
    /// <typeparam name="T2">元素2类型</typeparam>
    /// <typeparam name="T3">元素3类型</typeparam>
    public class Tuple<T1, T2, T3>
    {
        private T1 _item1;
        private T2 _item2;
        private T3 _item3;

        public T1 Item1 { get { return _item1; } }
        public T2 Item2 { get { return _item2; } }
        public T3 Item3 { get { return _item3; } }

        public Tuple(T1 item1, T2 item2, T3 item3)
        {
            _item1 = item1;
            _item2 = item2;
            _item3 = item3;
        }
    }

    /// <summary>
    /// 四元组
    /// </summary>
    /// <typeparam name="T1">元素1类型</typeparam>
    /// <typeparam name="T2">元素2类型</typeparam>
    /// <typeparam name="T3">元素3类型</typeparam>
    /// <typeparam name="T4">元素4类型</typeparam>
    public class Tuple<T1, T2, T3, T4>
    {
        private T1 _item1;
        private T2 _item2;
        private T3 _item3;
        private T4 _item4;

        public T1 Item1 { get { return _item1; } }
        public T2 Item2 { get { return _item2; } }
        public T3 Item3 { get { return _item3; } }
        public T4 Item4 { get { return _item4; } }

        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            _item1 = item1;
            _item2 = item2;
            _item3 = item3;
            _item4 = item4;
        }
    }

    /// <summary>
    /// 五元组
    /// </summary>
    /// <typeparam name="T1">元素1类型</typeparam>
    /// <typeparam name="T2">元素2类型</typeparam>
    /// <typeparam name="T3">元素3类型</typeparam>
    /// <typeparam name="T4">元素4类型</typeparam>
    /// <typeparam name="T5">元素5类型</typeparam>
    public class Tuple<T1, T2, T3, T4, T5>
    {
        private T1 _item1;
        private T2 _item2;
        private T3 _item3;
        private T4 _item4;
        private T5 _item5;

        public T1 Item1 { get { return _item1; } }
        public T2 Item2 { get { return _item2; } }
        public T3 Item3 { get { return _item3; } }
        public T4 Item4 { get { return _item4; } }
        public T5 Item5 { get { return _item5; } }

        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            _item1 = item1;
            _item2 = item2;
            _item3 = item3;
            _item4 = item4;
            _item5 = item5;
        }
    }

    /// <summary>
    /// 六元组
    /// </summary>
    /// <typeparam name="T1">元素1类型</typeparam>
    /// <typeparam name="T2">元素2类型</typeparam>
    /// <typeparam name="T3">元素3类型</typeparam>
    /// <typeparam name="T4">元素4类型</typeparam>
    /// <typeparam name="T5">元素5类型</typeparam>
    /// <typeparam name="T6">元素6类型</typeparam>
    public class Tuple<T1, T2, T3, T4, T5, T6>
    {
        private T1 _item1;
        private T2 _item2;
        private T3 _item3;
        private T4 _item4;
        private T5 _item5;
        private T6 _item6;

        public T1 Item1 { get { return _item1; } }
        public T2 Item2 { get { return _item2; } }
        public T3 Item3 { get { return _item3; } }
        public T4 Item4 { get { return _item4; } }
        public T5 Item5 { get { return _item5; } }
        public T6 Item6 { get { return _item6; } }

        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
        {
            _item1 = item1;
            _item2 = item2;
            _item3 = item3;
            _item4 = item4;
            _item5 = item5;
            _item6 = item6;
        }
    }

    public static class Tuple
    {
        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new Tuple<T1, T2>(item1, item2);
        }

        public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
        {
            return new Tuple<T1, T2, T3>(item1, item2, item3);
        }

        public static Tuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            return new Tuple<T1, T2, T3, T4>(item1, item2, item3, item4);
        }

        public static Tuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            return new Tuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
        }

        public static Tuple<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
        {
            return new Tuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, item5, item6);
        }
    }
#endif

    /// <summary>
    /// 二元组集合
    /// </summary>
    /// <typeparam name="T1">元素1类型</typeparam>
    /// <typeparam name="T2">元素2类型</typeparam>
    public class TupleList<T1, T2> : List<Tuple<T1, T2>>
    {
        public void Add(T1 item1, T2 item2)
        {
            base.Add(new Tuple<T1, T2>(item1, item2));
        }
    }

    public class RefCell<T> where T : struct
    {
        public T Contents { get; set; }

        public RefCell(T contents)
        {
            Contents = contents;
        }
    }
}
