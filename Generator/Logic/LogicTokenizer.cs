using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

#nullable enable
namespace TPRandomizer
{
    // Expression         := <Boolean> { <BooleanOperator> <Boolean> } ...
    // Boolean            := <BooleanConstant> | <ItemOrFunction> | <ProgressiveItem> | <Room> | <Setting> | "(" <Expression> ")"
    // BooleanOperator    := "and" | "or"
    // BooleanConstant    := "true" | "false"
    // ItemOrFunction     := <Name>
    // Room               := Room.<Name>
    // Setting            := "(" <Name> { "equals" | "not_equal" } <Value> ")"
    // ProgressiveItem    := "(" <Name> "," <Count> ")"

    /// <summary>
    /// Base class for logic parse tree nodes.
    /// </summary>
    public abstract class LogicAST
    {
        public abstract bool Evaluate();
    }

    namespace AST
    {
        public class True : LogicAST
        {
            public override bool Evaluate() => true;
        }

        public class False : LogicAST
        {
            public override bool Evaluate() => false;
        }

        public class Function : LogicAST
        {
            string FunctionName { get; }

            public Function(string function) => FunctionName = function;

            public override bool Evaluate()
            {
                MethodInfo? method = typeof(LogicFunctions).GetMethod(FunctionName);
                if (method == null)
                {
                    Console.WriteLine($"unknown logic function {FunctionName}");
                    return false;
                }

                object? result = method.Invoke(null, null);
                if (result is bool resultBool)
                {
                    return resultBool;
                }
                else
                {
                    Console.WriteLine($"logic function {FunctionName} returned non-bool {result}");
                    return false;
                }
            }
        }

        public class Item : LogicAST
        {
            TPRandomizer.Item ItemId { get; }
            int Count { get; }

            public Item(TPRandomizer.Item item, int count) => (ItemId, Count) = (item, count);

            public override bool Evaluate()
            {
                int heldCount = Randomizer.Items.heldItems.Where(i => i == ItemId).Count();
                // Console.WriteLine($"Item.Evaluate {heldCount} {Count} {ItemId}");
                return heldCount >= Count;
            }
        }

        public class Room : LogicAST
        {
            string RoomName { get; }

            public Room(string room) => RoomName = room;

            public override bool Evaluate() => Randomizer.Rooms.RoomDict[RoomName].ReachedByPlaythrough;
        }

        public class Setting : LogicAST
        {
            string SettingName { get; }
            string SettingValue { get; }
            bool Sense { get; }

            public Setting(string setting, string value, bool sense) => (SettingName, SettingValue, Sense) = (setting, value, sense);

            public override bool Evaluate() => LogicFunctions.EvaluateSetting(SettingName, SettingValue) == Sense;
        }

        public class Conjunction : LogicAST
        {
            LogicAST Left { get; }
            LogicAST Right { get; }

            public Conjunction(LogicAST left, LogicAST right) => (Left, Right) = (left, right);

            public override bool Evaluate() => Left.Evaluate() && Right.Evaluate();
        }

        public class Disjunction : LogicAST
        {
            LogicAST Left { get; }
            LogicAST Right { get; }

            public Disjunction(LogicAST left, LogicAST right) => (Left, Right) = (left, right);

            public override bool Evaluate() => Left.Evaluate() || Right.Evaluate();
        }
    }

    public class Parser
    {
        static Regex progressiveItemRegex = new(@"^\((\w+\s*),\s*(\d+)\)");
        static Regex settingRegex = new(@"^\(Setting.(\w+)\s+equals\s+(\w+)\)");
        static Regex settingInverseRegex = new(@"^\(Setting.(\w+)\s+not_equal\s+(\w+)\)");
        static Regex roomRegex = new(@"^Room.(\w+)");
        static Regex trueRegex = new(@"^true");
        static Regex falseRegex = new(@"^false");
        static Regex itemOrFunctionRegex = new(@"^(\w+)");
        static Regex conjunctionRegex = new(@"^and\s+");
        static Regex disjunctionRegex = new(@"^or\s+");
        static Dictionary<string, LogicAST> parseCache = [];

        /// <summary>
        /// Parses logic expressions into AST objects. This function uses an internal parse cache,
        /// but it's still better to cache the returned AST yourself if possible.
        /// </summary>
        /// <param name="expression">Logic expression as text.</param>
        /// <returns>Logic expression as an AST object.</returns>
        public static LogicAST Parse(string expression)
        {
            if (parseCache.TryGetValue(expression, out LogicAST? value))
            {
                return value;
            }

            string exprClone = expression;
            LogicAST parsed;
            try
            {
                parsed = ParseInner(ref exprClone, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to parse logic expression {expression}: {e}");
                throw;
            }

            parseCache[expression] = parsed;
            return parsed;
        }

        // this function does the actual work, because taking `ref string` on a public method
        // would be a little weird. internally, though, it's easier (and likely faster) to keep
        // one reference to expression and substring it as we consume characters.
        //
        // you could also do this with a pointer that gets passed to Match but it's more boilerplate
        static LogicAST ParseInner(ref string expression, int depth)
        {
            LogicAST? tree = null;

            while (expression.Length > 0)
            {
                expression = expression.Trim();
                Match? m;
                LogicAST thisNode;

                if ((m = Re(progressiveItemRegex, ref expression)) != null)
                {
                    thisNode = new AST.Item(Enum.Parse<Item>(m.Groups[1].Value), int.Parse(m.Groups[2].Value));
                }
                else if ((m = Re(settingRegex, ref expression)) != null)
                {
                    thisNode = new AST.Setting(m.Groups[1].Value, m.Groups[2].Value, true);
                }
                else if ((m = Re(settingInverseRegex, ref expression)) != null)
                {
                    thisNode = new AST.Setting(m.Groups[1].Value, m.Groups[2].Value, false);
                }
                else if ((m = Re(roomRegex, ref expression)) != null)
                {
                    thisNode = new AST.Room(m.Groups[1].Value.Replace('_', ' '));
                }
                else if (expression.StartsWith('('))
                {
                    // Start of a subexpression. We know it's not a progressive item check because
                    // we looked for that earlier.
                    expression = expression[1..];
                    thisNode = ParseInner(ref expression, depth + 1);
                    // skip the final )
                    if (expression.Length == 0 || expression[0] != ')')
                    {
                        throw new Exception("Expected closing parenthesis");
                    }
                    expression = expression[1..];
                }
                else if (Re(trueRegex, ref expression) != null)
                {
                    thisNode = new AST.True();
                }
                else if (Re(falseRegex, ref expression) != null)
                {
                    thisNode = new AST.False();
                }
                else if (Re(conjunctionRegex, ref expression) != null)
                {
                    thisNode = new AST.Conjunction(tree!, ParseInner(ref expression, depth));
                }
                else if (Re(disjunctionRegex, ref expression) != null)
                {
                    thisNode = new AST.Disjunction(tree!, ParseInner(ref expression, depth));
                }
                else if ((m = Re(itemOrFunctionRegex, ref expression)) != null)
                {
                    if (Enum.TryParse(m.Groups[1].Value, out Item item))
                    {
                        thisNode = new AST.Item(item, 1);
                    }
                    else
                    {
                        thisNode = new AST.Function(m.Groups[1].Value);
                    }
                }
                else if (expression.StartsWith(')'))
                {
                    if (depth > 0)
                    {
                        // end of a subexpression. let the caller handle advancing the read pointer
                        break;
                    }
                    else
                    {
                        throw new Exception("Unexpected closing parenthesis");
                    }
                }
                else
                {
                    Console.WriteLine($"failed to parse remainder of logic expression: {expression}");
                    expression = "";
                    thisNode = new AST.False();
                }

                tree = thisNode;
            }

            return tree!;
        }

        // helper function to match and advance or return null for comparison chains
        static Match? Re(Regex source, ref string expression)
        {
            Match m = source.Match(expression);
            if (m.Success)
            {
                expression = expression[m.Length..];
                return m;
            }
            else
            {
                return null;
            }
        }
    }
}
