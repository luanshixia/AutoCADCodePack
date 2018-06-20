//
// JigDrag类来源于明经通道
//

using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using System.Windows.Forms;
using System.Drawing;

namespace AutoCADCommands
{
    /// <summary>
    /// 封装简化Jig操作，函数式实现DrawJig
    /// </summary>
    public class JigDrag : DrawJig
    {
        protected JigDrag() { }

        #region =====封装Jig方法=====
        protected override bool WorldDraw(WorldDraw draw)
        {
            _callBack(new Result(_rst, draw));
            return true;
        }
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            return _acquireMod(prompts);
        }
        #endregion

        #region 定义各种Jig模式

        static PromptResult _rst;
        delegate SamplerStatus AcquireMod(JigPrompts prompts);
        //AcquirePoint
        static Point3d _point3d;
        static JigPromptPointOptions _PointOptions;
        static SamplerStatus GetPoint(JigPrompts prompts)
        {
            PromptPointResult rst = prompts.AcquirePoint(_PointOptions);
            _rst = rst;
            if (rst.Value != _point3d)
            {
                _point3d = rst.Value;
                return SamplerStatus.OK;
            }
            else
            {
                return SamplerStatus.NoChange;
            }
        }
        //AcquireDistance
        static Double _double;
        static JigPromptDistanceOptions _DistanceOptions;
        static SamplerStatus GetDistance(JigPrompts prompts)
        {
            var rst = prompts.AcquireDistance(_DistanceOptions);
            _rst = rst;
            if (rst.Value != _double)
            {
                _double = rst.Value;
                return SamplerStatus.OK;
            }
            else
            {
                return SamplerStatus.NoChange;
            }
        }
        //AcquireAngle
        static JigPromptAngleOptions _AngleOptions;
        static SamplerStatus GetAngle(JigPrompts prompts)
        {
            var rst = prompts.AcquireAngle(_AngleOptions);
            _rst = rst;
            if (rst.Value != _double)
            {
                _double = rst.Value;
                return SamplerStatus.OK;
            }
            else
            {
                return SamplerStatus.NoChange;
            }
        }
        //AcquireString
        static Point _cur;
        static JigPromptStringOptions _StringOptions;
        static SamplerStatus GetString(JigPrompts prompts)
        {
            var rst = prompts.AcquireString(_StringOptions);
            _rst = rst;
            var cur = Form.MousePosition;
            if (Form.MousePosition != _cur)
            {
                _cur = cur;
                return SamplerStatus.OK;
            }
            else
            {
                return SamplerStatus.NoChange;
            }
        }

        #endregion

        /// <summary>回调函数之参数</summary>
        public class Result
        {
            PromptResult rst;
            WorldDraw draw;
            internal Result(PromptResult promptResult, WorldDraw worldDraw)
            {
                rst = promptResult;
                draw = worldDraw;
            }
            /// <summary>原始返回值</summary>
            public PromptResult PromptResult
            {
                get { return rst; }
            }
            /// <summary>绘图对象</summary>
            public Geometry Geometry
            {
                get { return draw.Geometry; }
            }
            /// <summary>绘图方法</summary>
            public void Draw(Drawable entity)
            {
                if (draw != null)
                {
                    draw.Geometry.Draw(entity);
                }
            }
            /// <summary>GetPoint模式下的当前点位置</summary>
            public Point3d Point
            {
                get { return _point3d; }
            }
            /// <summary>GetDist模式下的当前距离</summary>
            public Double Dist
            {
                get { return _double; }
            }
            /// <summary>GetAngle模式下的当前角度</summary>
            public Double Angle
            {
                get { return _double; }
            }
            /// <summary>返回值指示</summary>
            public PromptStatus Status
            {
                get { return rst.Status; }
            }
            /// <summary>返回的字串or关键字</summary>
            public string String
            {
                get { return rst.StringResult; }
            }
        }
        static Action<Result> _callBack;
        static AcquireMod _acquireMod;

        /// <summary>简便快捷执行Jig拖动</summary>
        /// <param name="options">选项</param>
        /// <param name="callFun">回调函数</param>
        public static PromptResult StartDrag(JigPromptOptions options, Action<Result> callFun)
        {
            if (options is JigPromptPointOptions)
            {
                _PointOptions = options as JigPromptPointOptions;
                _acquireMod = GetPoint;
            }
            else if (options is JigPromptDistanceOptions)
            {
                _DistanceOptions = options as JigPromptDistanceOptions;
                _acquireMod = GetDistance;
            }
            else if (options is JigPromptAngleOptions)
            {
                _AngleOptions = options as JigPromptAngleOptions;
                _acquireMod = GetAngle;
            }
            else if (options is JigPromptStringOptions)
            {
                _StringOptions = options as JigPromptStringOptions;
                _acquireMod = GetString;
            }
            _callBack = callFun;
            Autodesk.AutoCAD.ApplicationServices.Application.
                DocumentManager.MdiActiveDocument.Editor.Drag(new JigDrag());
            _callBack(new Result(_rst, null));
            return _rst;
        }

        /// <summary>简便快捷执行Jig拖动[Point模式]</summary>
        /// <param name="msg">提示信息</param>
        /// <param name="kwd">关键字</param>
        /// <param name="callFun">回调函数</param>
        public static PromptResult StartDrag(string msg, string kwd, Action<Result> callFun)
        {
            return StartDrag(new JigPromptPointOptions(msg, kwd), callFun);
        }

        /// <summary>简便快捷执行Jig拖动[Point模式]</summary>
        /// <param name="msg">提示信息</param>
        /// <param name="callFun">回调函数</param>
        public static PromptResult StartDrag(string msg, Action<Result> callFun)
        {
            return StartDrag(new JigPromptPointOptions(msg), callFun);
        }
    }
}
