using System.Linq;
using System.Text;
using mc_compiled.MCC.Compiler;

namespace mc_compiled.Commands;

/// <summary>
///     Builds a '/camera' command.
/// </summary>
public class CameraBuilder
{
    private static readonly string[] ENTITY_OFFSET_ALLOWED = ["minecraft:follow_orbit"];
    private static readonly string[] VIEW_OFFSET_ALLOWED = ENTITY_OFFSET_ALLOWED;

    public readonly string players;
    public readonly string preset;
    public decimal? easeDuration;
    public Easing? easePreset;
    public decimal? entityOffsetX;
    public decimal? entityOffsetY;
    public decimal? entityOffsetZ;
    public string facingEntity;
    public Coordinate? facingX;
    public Coordinate? facingY;
    public Coordinate? facingZ;
    public Coordinate? positionX;
    public Coordinate? positionY;
    public Coordinate? positionZ;
    public Coordinate? rotationX;
    public Coordinate? rotationY;
    public bool useDefault;
    public bool useEasing;
    public bool useEntityOffset;
    public bool useFacing;
    public bool usePosition;
    public bool useRotation;
    public bool useViewOffset;
    public Coordinate? viewOffsetX;
    public Coordinate? viewOffsetY;

    public CameraBuilder(string players, string preset)
    {
        this.players = players;
        this.preset = preset;

        this.useDefault = true;

        this.useEasing = false;
        this.useEntityOffset = false;
        this.useFacing = false;
        this.usePosition = false;
        this.useRotation = false;
        this.useViewOffset = false;
    }
    private static string EntityOffsetAllowedString => string.Join(", ", ENTITY_OFFSET_ALLOWED);
    private static string ViewOffsetAllowedString => string.Join(", ", VIEW_OFFSET_ALLOWED);
    public CameraBuilder WithEasing(Easing easing, decimal duration, Statement callingStatement)
    {
        if (this.useViewOffset || this.useEntityOffset)
            throw new StatementException(callingStatement, "Cannot use easing alongside view_offset or entity_offset.");

        this.useEasing = true;
        this.easePreset = easing;
        this.easeDuration = duration;
        return this;
    }

    public CameraBuilder WithEntityOffset(decimal x, decimal y, decimal z, Statement callingStatement)
    {
        if (this.usePosition)
            throw new StatementException(callingStatement, "Cannot use position alongside entity_offset.");
        if (this.useEasing)
            throw new StatementException(callingStatement, "Cannot use easing alongside entity_offset.");
        if (!ENTITY_OFFSET_ALLOWED.Contains(this.preset))
            throw new StatementException(callingStatement,
                $"Cannot use entity offset with preset '{this.preset}'. Allowed presets: {EntityOffsetAllowedString}.");

        this.useDefault = false;
        this.useEntityOffset = true;
        this.entityOffsetX = x;
        this.entityOffsetY = y;
        this.entityOffsetZ = z;
        return this;
    }
    public CameraBuilder WithEntityOffset(Coordinate x, Coordinate y, Coordinate z, Statement callingStatement)
    {
        if (this.usePosition)
            throw new StatementException(callingStatement, "Cannot use position alongside entity_offset.");
        if (this.useEasing)
            throw new StatementException(callingStatement, "Cannot use easing alongside entity_offset.");
        if (!ENTITY_OFFSET_ALLOWED.Contains(this.preset))
            throw new StatementException(callingStatement,
                $"Cannot use entity offset with preset '{this.preset}'. Allowed presets: {EntityOffsetAllowedString}.");

        this.useDefault = false;
        this.useEntityOffset = true;
        this.entityOffsetX = x.isDecimal ? x.valueDecimal : x.valueInteger;
        this.entityOffsetY = y.isDecimal ? y.valueDecimal : y.valueInteger;
        this.entityOffsetZ = z.isDecimal ? z.valueDecimal : z.valueInteger;
        return this;
    }
    public CameraBuilder WithViewOffset(decimal x, decimal y, Statement callingStatement)
    {
        if (this.usePosition)
            throw new StatementException(callingStatement, "Cannot use position alongside view_offset.");
        if (this.useEasing)
            throw new StatementException(callingStatement, "Cannot use easing alongside view_offset.");
        if (!VIEW_OFFSET_ALLOWED.Contains(this.preset))
            throw new StatementException(callingStatement,
                $"Cannot use view offset with preset '{this.preset}'. Allowed presets: {ViewOffsetAllowedString}.");

        this.useDefault = false;
        this.useViewOffset = true;
        this.viewOffsetX = x;
        this.viewOffsetY = y;
        return this;
    }
    public CameraBuilder WithViewOffset(Coordinate x, Coordinate y, Statement callingStatement)
    {
        if (this.usePosition)
            throw new StatementException(callingStatement, "Cannot use position alongside view_offset.");
        if (this.useEasing)
            throw new StatementException(callingStatement, "Cannot use easing alongside view_offset.");
        if (!VIEW_OFFSET_ALLOWED.Contains(this.preset))
            throw new StatementException(callingStatement,
                $"Cannot use view offset with preset '{this.preset}'. Allowed presets: {ViewOffsetAllowedString}.");

        this.useDefault = false;
        this.useViewOffset = true;
        this.viewOffsetX = x.isDecimal ? x.valueDecimal : x.valueInteger;
        this.viewOffsetY = y.isDecimal ? y.valueDecimal : y.valueInteger;
        return this;
    }

    public CameraBuilder WithFacing(Coordinate x, Coordinate y, Coordinate z, Statement callingStatement)
    {
        if (this.useRotation)
            throw new StatementException(callingStatement,
                "Cannot use rotation and facing parameters at the same time.");

        this.useDefault = false;
        this.useFacing = true;
        this.facingEntity = null;
        this.facingX = x;
        this.facingY = y;
        this.facingZ = z;
        return this;
    }
    public CameraBuilder WithFacing(string entity, Statement callingStatement)
    {
        if (this.useRotation)
            throw new StatementException(callingStatement,
                "Cannot use rotation and facing parameters at the same time.");

        this.useDefault = false;
        this.useFacing = true;
        this.facingEntity = entity;
        this.facingX = null;
        this.facingY = null;
        this.facingZ = null;
        return this;
    }
    public CameraBuilder WithPosition(Coordinate x, Coordinate y, Coordinate z, Statement callingStatement)
    {
        if (this.useViewOffset || this.useEntityOffset)
            throw new StatementException(callingStatement,
                "Cannot use position alongside view_offset or entity_offset.");

        this.useDefault = false;
        this.usePosition = true;
        this.positionX = x;
        this.positionY = y;
        this.positionZ = z;
        return this;
    }
    public CameraBuilder WithRotation(Coordinate xRotation, Coordinate yRotation, Statement callingStatement)
    {
        if (this.useFacing)
            throw new StatementException(callingStatement,
                "Cannot use rotation and facing parameters at the same time.");

        this.useDefault = false;
        this.useRotation = true;
        this.rotationX = xRotation;
        this.rotationY = yRotation;
        return this;
    }

    public string Build()
    {
        var sb = new StringBuilder();
        sb.Append("camera ");
        sb.Append(this.players);
        sb.Append(" set ");
        sb.Append(this.preset);
        sb.Append(' ');

        void AppendPosition()
        {
            sb.Append("pos ");
            sb.Append(this.positionX!.Value);
            sb.Append(' ');
            sb.Append(this.positionY!.Value);
            sb.Append(' ');
            sb.Append(this.positionZ!.Value);
            sb.Append(' ');
        }

        void AppendFacing()
        {
            if (this.facingEntity != null)
            {
                sb.Append("facing ");
                sb.Append(this.facingEntity);
            }
            else
            {
                sb.Append("facing ");
                sb.Append(this.facingX!.Value);
                sb.Append(' ');
                sb.Append(this.facingY!.Value);
                sb.Append(' ');
                sb.Append(this.facingZ!.Value);
            }

            sb.Append(' ');
        }

        void AppendRotation()
        {
            sb.Append("rot ");
            sb.Append(this.rotationX!.Value.ToString());
            sb.Append(' ');
            sb.Append(this.rotationY!.Value.ToString());
            sb.Append(' ');
        }

        void AppendEntityOffset()
        {
            sb.Append("entity_offset ");
            sb.Append(this.entityOffsetX!.Value);
            sb.Append(' ');
            sb.Append(this.entityOffsetY!.Value);
            sb.Append(' ');
            sb.Append(this.entityOffsetZ!.Value);
            sb.Append(' ');
        }

        void AppendViewOffset()
        {
            sb.Append("view_offset ");
            sb.Append(this.viewOffsetX!.Value);
            sb.Append(' ');
            sb.Append(this.viewOffsetY!.Value);
            sb.Append(' ');
        }

        if (this.useEasing)
        {
            sb.Append("ease ");
            sb.Append(this.easeDuration!.Value);
            sb.Append(' ');
            sb.Append(this.easePreset!.Value.ToString().ToLower());
            sb.Append(' ');
        }

        if (this.useDefault)
        {
            sb.Append("default");
            return sb.ToString();
        }

        if (this.usePosition)
        {
            AppendPosition();
            if (this.useRotation)
                AppendRotation();
            if (this.useFacing)
                AppendFacing();
            return sb.ToString().TrimEnd();
        }

        if (this.useRotation)
        {
            AppendRotation();
            if (this.useViewOffset)
                AppendViewOffset();
            if (this.useEntityOffset)
                AppendEntityOffset();
            return sb.ToString().TrimEnd();
        }

        if (this.useFacing)
        {
            AppendFacing();
            return sb.ToString().TrimEnd();
        }

        if (this.useViewOffset)
        {
            AppendViewOffset();
            if (this.useEntityOffset)
                AppendEntityOffset();
            return sb.ToString().TrimEnd();
        }

        if (this.useEntityOffset)
        {
            AppendEntityOffset();
            return sb.ToString().TrimEnd();
        }

        return sb.ToString().TrimEnd();
    }
}