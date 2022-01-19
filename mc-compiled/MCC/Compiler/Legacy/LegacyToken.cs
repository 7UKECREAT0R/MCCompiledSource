using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler
{
    public abstract class LegacyToken
    {
        public LEGACYTOKENTYPE type;
        public int line = -1;

        public abstract void Execute(LegacyExecutor caller, LegacyTokenFeeder tokens);
    }
    public class TokenException : Exception
    {
        public LegacyToken token;
        public string desc;

        /// <summary>
        /// Indicates that a token has encountered a managed error while executing.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="desc"></param>
        public TokenException(LegacyToken token, string desc)
        {
            this.token = token;
            this.desc = desc;
        }
    }
    public enum LEGACYTOKENTYPE : byte
    {
        // Region 0x0 - Internal
        UNKNOWN = 0x00,
        COMMENT = 0x01,
        BLOCK = 0x02,

        // Region 0x1 - Preprocessor
        PPV = 0x10,         // Assign                   PPV <dst> <src>
        PPINC = 0x11,       // Increment                PPINC <var>
        PPDEC = 0x12,       // Decrement                PPDEC <var>
        PPADD = 0x13,       // Add                      PPADD <dst> <src>
        PPSUB = 0x14,       // Subtract                 PPSUB <dst> <src>
        PPMUL = 0x15,       // Multiply                 PPMUL <dst> <src>
        PPDIV = 0x16,       // Divide                   PPDIV <dst> <src>
        PPMOD = 0x17,       // Modulo                   PPMOD <dst> <src>
        PPIF = 0x18,        // If (Preprocessor)        PPIF <A> <,<=,=,!=,>,>= <B>
        PPELSE = 0x19,      // Else (Preprocessor)      PPELSE
        PPREP = 0x1A,       // Repeat Block             PPREP <amount>
        PPLOG = 0x1B,       // Log to Console           PPLOG <str>
        PPMACRO = 0x1C,     // Set or Invoke Macro      PPMACRO <name> <args>
        PPINCLUDE = 0x1D,   // Include MCC File         PPINCLUDE <file>
        FUNCTION = 0x1E,    // Define Function          FUNCTION [modifiers...] <name> [args...]
        CALL = 0x1F,        // Call Function            CALL <name> [args...]

        // Region 0x2 - Preprocessor Extensions
        HALT = 0x20,        // Halt Execution                           HALT
        PPFRIENDLY = 0x21,  // Convert Value to Friendly Name           PPFRIENDLY <var>
        PPUPPER = 0x22,     // Convert Value to UPPERCASE               PPUPPER <var>
        PPLOWER = 0x23,     // Convert Value to lowercase               PPLOWER <var>

        // Region 0x3-0x6 - Minecraft
        MC = 0x30,          // Append regular minecraft command.            MC <command>
        SELECT = 0x31,      // Set the selected entity.         
                            //      SELECT @<s|e|a|r|p>
        PRINT = 0x32,       // Print to all players.                        PRINT <str>
        PRINTP = 0x33,      // Print to the selected entity.                PRINTP <str>
        _LIMIT = 0x34,      // OBSOLETE: USE "if limit x"
                            //      LIMIT <amount>
                            //      LIMIT NONE
        DEFINE = 0x35,      // Define a value to be used.                   DEFINE <valuename>
        INITIALIZE = 0x36,  // Initialize a value for all players.          INITIALIZE <valuename>
        
        VALUE = 0x37,       // Modify a value.
                            //      VALUE <valuename> ADD <integer/othervalue>
                            //      VALUE <valuename> SUB <integer/othervalue> 
                            //      VALUE <valuename> MUL <integer/othervalue>els
                            //      VALUE <valuename> DIV <integer/othervalue>
                            //      VALUE <valuename> MOD <integer/othervalue>
                            //      VALUE <valuename> SET <integer/othervalue>
                            //      VALUE <valuename> += <integer/othervalue>
                            //      VALUE <valuename> -= <integer/othervalue> 
                            //      VALUE <valuename> *= <integer/othervalue>
                            //      VALUE <valuename> /= <integer/othervalue>
                            //      VALUE <valuename> %= <integer/othervalue>
                            //      VALUE <valuename> = <integer/othervalue>

        IF = 0x38,          // Compare different aspects of the selected player. Appends on to the existing selector.
                            //      IF <valuename> <,<=,=,!=,>,>= <integer>         Check for a scoreboard value.
                            //      IF BLOCK <x> <y> <z> <blockname> [data]         Check for a block at coordinate.
                            //      IF TYPE <type>                                  Check if the selected entity is of type.
                            //      IF FAMILY <family>                              Check if the selected entity is of family. (eg. mob, animal)
                            //      IF TAG <tagname>                                Check if entity has tag.
                            //      IF MODE <gamemode>                              Check if player is in gamemode.
                            //      IF NEAR <x> <y> <z> <radius> [radiusMin]        Check if entity is near location.
                            //      IF INSIDE <sizeX> <sizeY> <sizeZ> [x] [y] [z]       Check if entity is in area. If size is 0 then it won't be set.
                            //      IF LEVEL <min> [max]                            Check if player has level.
                            //      IF NAME <string>                                Check if entity has name.
                            //      IF LIMIT <amount>                               Only execute on a certain number of entities.

                            //      You can combine them by using & as a delimiter.
                            //          if block ~ ~-1 ~ lava & tag lavawalker
                            //      You can also invert the condition by placing "not" before it.
                            //          if not name Jeremy The Pig

        ELSE = 0x39,        // Used after an IF-family statement to check for the inverse.
        GIVE = 0x3A,        // Give the selected player an item.
                            //      GIVE <item> [count] [damage] [keep? lockinventory? lockslot? [canplaceon <block>]
                            //          [candestroy <block>] [enchant <enchant> <level>] [name <itemname>]]
        TP = 0x3B,          // Teleport the selected player/entity.
                            //      TP <x> <y> <z> [yRot] [xRot]
                            //      TP @<selector>
        MOVE = 0x3C,        // Move the selected entity in a direction.
                            //      MOVE <up|down|left|right|forward|backward> <amount>
        FACE = 0x3D,        // Face a location or entity.
                            //      FACE <x> <y> <z>
                            //      FACE @<selector>
        PLACE = 0x3E,       // Place a block in the world.
                            //      PLACE <block> <x> <y> <z> [data] [destroyMode]
        FILL = 0x3F,        // Fill an area with blocks.
                            //      FILL <block> <x1> <y1> <z1> <x2> <y2> <z2> [data] [fillMode]
        REPLACE = 0x40,     // Replace an area's specific blocks with another.
                            //      REPLACE <x1> <y1> <z1> <x2> <y2> <z2> <source> <data> <replacement> [data]
        KILL = 0x41,        // Kill selected entity with optional delay
                            // The delay should use a time parsing class for stuff like "5s" or "1m"
                            //      KILL [delay]
        TITLE = 0x42        // Send the selected entity a title. Optionally can specify timings and subtitles.
                            //      TITLE <text> [subtitle: <subtitle>] [times: <fade-in> <stay> <fade-out>]


        // Planned stuff:

        // BINARY WRITE <src> <dst>
        //      Convert value to 32 binary scoreboard values.
        // BINARY READ <src> <dst>
        //      Convert binary number back to scoreboard value.
    }
}
