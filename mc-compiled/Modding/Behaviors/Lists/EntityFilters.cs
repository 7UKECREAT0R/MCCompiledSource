using mc_compiled.Commands;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Behaviors.Lists;

public class FilterClockTime : Filter
{
    public const float NOON = 0.0f;
    public const float SUNSET = 0.25f;
    public const float MIDNIGHT = 0.5f;
    public const float SUNRISE = 0.75f;

    public float time; // 0.0 - 1.0
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "clock_time";
    }
    public override object GetValue()
    {
        return this.time;
    }
}

public class FilterDistanceToNearestPlayer : Filter
{
    public float distance;
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "distance_to_nearest_player";
    }
    public override object GetValue()
    {
        return this.distance;
    }
}

public class FilterHasAbility : Filter
{
    public enum Ability
    {
        flyspeed,
        flying,
        instabuild,
        invulnerable,
        lightning,
        mayfly,
        mute,
        noclip,
        walkspeed,
        worldbuilder
    }

    public Ability ability; // 0.0 - 1.0
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "has_ability";
    }
    public override object GetValue()
    {
        return this.ability.ToString();
    }
}

public class FilterHasBiomeTag : Filter
{
    public string biomeTag;
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "has_biome_tag";
    }
    public override object GetValue()
    {
        return this.biomeTag;
    }
}

public class FilterHasComponent : Filter
{
    public string componentName;
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "has_component";
    }
    public override object GetValue()
    {
        return this.componentName;
    }
}

public class FilterHasContainerOpen : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "has_container_open";
    }
}

public class FilterHasDamage : Filter
{
    public DamageCause damageType;
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "has_damage";
    }
    public override object GetValue()
    {
        return this.damageType.ToString();
    }
}

public class FilterHasEquipment : Filter
{
    public enum Domain
    {
        any,
        armor,
        feet,
        hand,
        head,
        leg,
        torso
    }

    public Domain? itemDomain;

    public string itemName;
    public override JProperty[] GetExtraProperties()
    {
        if (this.itemDomain.HasValue)
            return
            [
                new JProperty("domain", this.itemDomain.ToString())
            ];

        return null;
    }
    public override string GetTest()
    {
        return "has_equipment";
    }
    public override object GetValue()
    {
        return this.itemName;
    }
}

public class FilterHasMobEffect : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "has_mob_effect";
    }
}

public class FilterHasNametag : Filter
{
    public string nameTag;
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "has_nametag";
    }
    public override object GetValue()
    {
        return this.nameTag;
    }
}

public class FilterHasRangedWeapon : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "has_ranged_weapon";
    }
}

public class FilterHasTag : Filter
{
    public string tag;
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "has_tag";
    }
    public override object GetValue()
    {
        return this.tag;
    }
}

public class FilterHasTarget : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "has_target";
    }
}

public class FilterHasTradeSupply : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "has_trade_supply";
    }
}

public class FilterHourlyClockTime : Filter
{
    public const int MIN_TIME = 0;
    public const int MAX_TIME = 24000;

    protected int time;
    public int Time
    {
        get => this.time;
        set
        {
            if (value > MAX_TIME)
                value = MAX_TIME;
            if (value < MIN_TIME)
                value = MIN_TIME;
            this.time = value;
        }
    }

    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "hourly_clock_time";
    }
    public override object GetValue()
    {
        return this.time;
    }
}

public class FilterInCaravan : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "in_caravan";
    }
}

public class FilterInClouds : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "in_clouds";
    }
}

public class FilterInLava : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "in_lava";
    }
}

public class FilterInNether : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "in_nether";
    }
}

public class FilterInWater : BooleanFilter
{
    public bool includeRain;
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return this.includeRain ? "in_water_or_rain" : "in_water";
    }
}

public class FilterInactivityTimer : Filter
{
    public int inactiveFor;
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "inactivity_timer";
    }
    public override object GetValue()
    {
        return this.inactiveFor;
    }
}

public class FilterIsAltitude : Filter
{
    public int yValue;
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_altitude";
    }
    public override object GetValue()
    {
        return this.yValue;
    }
}

public class FilterIsAvoidingMobs : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_avoiding_mobs";
    }
}

public class FilterIsBiome : Filter
{
    public string biome;
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_biome";
    }
    public override object GetValue()
    {
        return this.biome;
    }
}

public class FilterIsBlock : Filter
{
    public string blockName;
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_block";
    }
    public override object GetValue()
    {
        return this.blockName;
    }
}

public class FilterIsBrightness : Filter
{
    public float brightness; // 0.0 - 1.0
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_brightness";
    }
    public override object GetValue()
    {
        return this.brightness;
    }
}

public class FilterIsClimbing : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_climbing";
    }
}

public class FilterIsColor : Filter
{
    public enum FilterColor
    {
        black,
        blue,
        brown,
        cyan,
        gray,
        green,
        light_blue,
        light_green,
        magenta,
        orange,
        pink,
        purple,
        red,
        silver,
        white,
        yellow
    }

    public FilterColor color;
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_color";
    }
    public override object GetValue()
    {
        return this.color.ToString();
    }
}

public class FilterIsDaytime : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_daytime";
    }
}

public class FilterIsDifficulty : Filter
{
    public DifficultyMode difficulty;
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_difficulty";
    }
    public override object GetValue()
    {
        return this.difficulty.ToString();
    }
}

public class FilterIsFamily : Filter
{
    public string family;
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_family";
    }
    public override object GetValue()
    {
        return this.family;
    }
}

public class FilterIsGameRule : Filter
{
    public GameRule gameRule;
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_game_rule";
    }
    public override object GetValue()
    {
        return this.gameRule.ToString();
    }
}

public class FilterIsHumid : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_humid";
    }
}

public class FilterIsImmobile : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_immobile";
    }
}

public class FilterIsInVillage : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_village";
    }
}

public class FilterIsLeashed : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_leashed";
    }
}

public class FilterIsLeashedTo : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_leashed_to";
    }
}

public class FilterIsMarkVariant : Filter
{
    public int markVariant;
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_mark_variant";
    }
    public override object GetValue()
    {
        return this.markVariant;
    }
}

public class FilterIsMoving : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_moving";
    }
}

public class FilterIsOwner : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_owner";
    }
}

public class FilterIsPersistent : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_persistent";
    }
}

public class FilterIsRiding : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_riding";
    }
}

public class FilterIsSkinID : Filter
{
    public int skinID;
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_skin_id";
    }
    public override object GetValue()
    {
        return this.skinID;
    }
}

public class FilterIsSleeping : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_sleeping";
    }
}

public class FilterIsSneaking : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_sneaking";
    }
}

public class FilterIsSnowCovered : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_snow_covered";
    }
}

public class FilterIsTarget : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_target";
    }
}

public class FilterIsTemperatureType : Filter
{
    public enum TemperatureType
    {
        cold,
        mild,
        ocean,
        warm
    }

    public TemperatureType type;
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_temperature_type";
    }
    public override object GetValue()
    {
        return this.type.ToString();
    }
}

public class FilterIsTemperatureValue : Filter
{
    public float temperature; // 0.0 - 1.0
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_temperature_value";
    }
    public override object GetValue()
    {
        return this.temperature;
    }
}

public class FilterIsUnderground : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_underground";
    }
}

public class FilterIsUnderwater : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_underwater";
    }
}

public class FilterIsVariant : Filter
{
    public int variant;
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_variant";
    }
    public override object GetValue()
    {
        return this.variant;
    }
}

public class FilterIsVisible : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "is_visible";
    }
}

public class FilterLightLevel : Filter
{
    public int lightLevel;
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "light_level";
    }
    public override object GetValue()
    {
        return this.lightLevel;
    }
}

public class FilterMoonIntensity : Filter
{
    public float moonIntensity; // 0.0 - 1.0
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "moon_intensity";
    }
    public override object GetValue()
    {
        return this.moonIntensity;
    }
}

public class FilterMoonPhase : Filter
{
    public const int MIN_PHASE = 0;
    public const int MAX_PHASE = 7;

    private int moonPhase;
    public int MoonPhase
    {
        get => this.moonPhase;
        set
        {
            if (value > MAX_PHASE)
                value = MAX_PHASE;
            if (value < MIN_PHASE)
                value = MIN_PHASE;
            this.moonPhase = value;
        }
    }

    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "moon_phase";
    }
    public override object GetValue()
    {
        return this.moonPhase;
    }
}

public class FilterOnGround : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "on_ground";
    }
}

public class FilterOnLadder : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "on_ladder";
    }
}

public class FilterRandomChance : Filter
{
    public int randomChance; // random(0..randomChance) == 0
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "random_chance";
    }
    public override object GetValue()
    {
        return this.randomChance;
    }
}

public class FilterRiderCount : Filter
{
    public int count;
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "rider_count";
    }
    public override object GetValue()
    {
        return this.count;
    }
}

public class FilterIsSurfaceMob : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "surface_mob";
    }
}

public class FilterTrusts : BooleanFilter
{
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return "trusts";
    }
}

public class FilterWeather : Filter
{
    public bool atPosition;
    public WeatherState weather;
    public override JProperty[] GetExtraProperties()
    {
        return null;
    }
    public override string GetTest()
    {
        return this.atPosition ? "weather_at_position" : "weather";
    }
    public override object GetValue()
    {
        return this.weather.ToString();
    }
}