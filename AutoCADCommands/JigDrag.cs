using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using System;
using System.Drawing;
using Form = System.Windows.Forms.Form;

namespace AutoCADCommands
{
    /// <summary>
    /// The JigDrag helper.
    /// </summary>
    public class JigDrag : DrawJig
    {
        static PromptResult _result;
        static Action<JigDragResult> _callback;
        static Func<JigPrompts, SamplerStatus> _acquireMod;

        protected JigDrag() { }

        protected override bool WorldDraw(WorldDraw draw)
        {
            _callback(new JigDragResult(_result, draw));
            return true;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            return _acquireMod(prompts);
        }

        // Point
        static Point3d _point3d;
        static JigPromptPointOptions _PointOptions;
        static SamplerStatus GetPoint(JigPrompts prompts)
        {
            var rst = prompts.AcquirePoint(_PointOptions);
            _result = rst;
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

        // Distance
        static Double _double;
        static JigPromptDistanceOptions _DistanceOptions;
        static SamplerStatus GetDistance(JigPrompts prompts)
        {
            var rst = prompts.AcquireDistance(_DistanceOptions);
            _result = rst;
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

        // Angle
        static JigPromptAngleOptions _AngleOptions;
        static SamplerStatus GetAngle(JigPrompts prompts)
        {
            var rst = prompts.AcquireAngle(_AngleOptions);
            _result = rst;
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

        // String
        static Point _cur;
        static JigPromptStringOptions _StringOptions;
        static SamplerStatus GetString(JigPrompts prompts)
        {
            var rst = prompts.AcquireString(_StringOptions);
            _result = rst;
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

        /// <summary>
        /// The JigDrag result.
        /// </summary>
        public class JigDragResult
        {
            PromptResult result;
            WorldDraw draw;

            internal JigDragResult(PromptResult promptResult, WorldDraw worldDraw)
            {
                result = promptResult;
                draw = worldDraw;
            }

            /// <summary>原始返回值</summary>
            public PromptResult PromptResult
            {
                get { return result; }
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
                get { return result.Status; }
            }

            /// <summary>返回的字串or关键字</summary>
            public string String
            {
                get { return result.StringResult; }
            }
        }

        /// <summary>简便快捷执行Jig拖动</summary>
        /// <param name="options">选项</param>
        /// <param name="callback">回调函数</param>
        public static PromptResult StartDrag(JigPromptOptions options, Action<JigDragResult> callback)
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
            _callback = callback;
            Application.DocumentManager.MdiActiveDocument.Editor.Drag(new JigDrag());
            _callback(new JigDragResult(_result, null));
            return _result;
        }

        /// <summary>简便快捷执行Jig拖动[Point模式]</summary>
        /// <param name="msg">提示信息</param>
        /// <param name="kwd">关键字</param>
        /// <param name="callback">回调函数</param>
        public static PromptResult StartDrag(string msg, string kwd, Action<JigDragResult> callback)
        {
            return JigDrag.StartDrag(new JigPromptPointOptions(msg, kwd), callback);
        }

        /// <summary>简便快捷执行Jig拖动[Point模式]</summary>
        /// <param name="msg">提示信息</param>
        /// <param name="callback">回调函数</param>
        public static PromptResult StartDrag(string msg, Action<JigDragResult> callback)
        {
            return JigDrag.StartDrag(new JigPromptPointOptions(msg), callback);
        }
    }
}
