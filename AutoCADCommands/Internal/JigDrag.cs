using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsInterface;
using System;

namespace Dreambuild.AutoCAD.Internal
{
    internal class FlexEntityJig : EntityJig
    {
        protected JigPromptOptions Options { get; }

        protected Func<Entity, PromptResult, bool> UpdateAction { get; }

        protected PromptResult JigResult { get; set; }

        protected string JigResultValue { get; set; }

        public FlexEntityJig(
            JigPromptOptions options,
            Entity entity,
            Func<Entity, PromptResult, bool> updateAction)
            : base(entity)
        {
            this.Options = options;
            this.UpdateAction = updateAction;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            string jigResultValue = null;

            if (this.Options is JigPromptPointOptions pointOptions)
            {
                var result = prompts.AcquirePoint(pointOptions);
                this.JigResult = result;
                jigResultValue = result.Value.ToString();
            }
            else if (this.Options is JigPromptDistanceOptions distanceOptions)
            {
                var result = prompts.AcquireDistance(distanceOptions);
                this.JigResult = result;
                jigResultValue = result.Value.ToString();
            }
            else if (this.Options is JigPromptAngleOptions angleOptions)
            {
                var result = prompts.AcquireAngle(angleOptions);
                this.JigResult = result;
                jigResultValue = result.Value.ToString();
            }
            else if (this.Options is JigPromptStringOptions stringOptions)
            {
                var result = prompts.AcquireString(stringOptions);
                this.JigResult = result;
                jigResultValue = result.StringResult;
            }

            if (jigResultValue == null)
            {
                return SamplerStatus.Cancel;
            }
            else if (jigResultValue != this.JigResultValue)
            {
                this.JigResultValue = jigResultValue;
                return SamplerStatus.OK;
            }

            return SamplerStatus.NoChange;
        }

        protected override bool Update()
        {
            return this.UpdateAction(base.Entity, this.JigResult);
        }
    }

    internal class FlexDrawJig : DrawJig
    {
        protected JigPromptOptions Options { get; }

        protected Func<PromptResult, Drawable> UpdateAction { get; }

        protected PromptResult JigResult { get; set; }

        protected string JigResultValue { get; set; }

        public FlexDrawJig(
            JigPromptOptions options,
            Func<PromptResult, Drawable> updateAction)
        {
            this.Options = options;
            this.UpdateAction = updateAction;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            string jigResultValue = null;

            if (this.Options is JigPromptPointOptions pointOptions)
            {
                var result = prompts.AcquirePoint(pointOptions);
                this.JigResult = result;
                jigResultValue = result.Value.ToString();
            }
            else if (this.Options is JigPromptDistanceOptions distanceOptions)
            {
                var result = prompts.AcquireDistance(distanceOptions);
                this.JigResult = result;
                jigResultValue = result.Value.ToString();
            }
            else if (this.Options is JigPromptAngleOptions angleOptions)
            {
                var result = prompts.AcquireAngle(angleOptions);
                this.JigResult = result;
                jigResultValue = result.Value.ToString();
            }
            else if (this.Options is JigPromptStringOptions stringOptions)
            {
                var result = prompts.AcquireString(stringOptions);
                this.JigResult = result;
                jigResultValue = result.StringResult;
            }

            if (jigResultValue == null)
            {
                return SamplerStatus.Cancel;
            }
            else if (jigResultValue != this.JigResultValue)
            {
                this.JigResultValue = jigResultValue;
                return SamplerStatus.OK;
            }

            return SamplerStatus.NoChange;
        }

        protected override bool WorldDraw(WorldDraw draw)
        {
            return draw.Geometry.Draw(this.UpdateAction(this.JigResult));
        }
    }
}
