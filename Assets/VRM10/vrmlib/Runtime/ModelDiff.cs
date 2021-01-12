using System;
using System.Collections.Generic;

namespace VrmLib.Diff
{
    static class ObjectExtensions
    {
        public static bool IsNull<T>(this T self)
        {
            if (typeof(T).IsClass)
            {
                return self == null;
            }
            else
            {
                return false;
            }
        }
    }

    public struct ModelDiff
    {
        public string Context;
        public string Message;

        public override string ToString()
        {
            return $"{Context}: {Message}";
        }
    }

    public struct ModelDiffContext
    {
        public readonly string Path;

        public readonly List<ModelDiff> List;

        ModelDiffContext(string path, List<ModelDiff> list)
        {
            Path = path;
            List = list;
        }

        public bool Push<T>(T lhs, T rhs, Func<ModelDiffContext, T, T, bool> pred = null)
        {
            if (pred != null)
            {
                if (!pred(this, lhs, rhs))
                {
                    List.Add(new ModelDiff
                    {
                        Context = Path,
                        Message = $"{lhs} != {rhs}",
                    });
                    return false;
                }
                return true;
            }

            if (!RequireComapre(lhs, rhs, out bool equals))
            {
                return equals;
            }

            if (lhs.Equals(rhs))
            {
                return true;
            }
            else
            {
                List.Add(new ModelDiff
                {
                    Context = Path,
                    Message = $"{lhs} != {rhs}",
                });
                return false;
            }
        }

        public ModelDiffContext Enter(string key)
        {
            if (string.IsNullOrEmpty(Path))
            {
                return new ModelDiffContext(key, List);
            }
            else
            {
                return new ModelDiffContext(Path + "." + key, List);
            }
        }

        public static ModelDiffContext Create()
        {
            return new ModelDiffContext("", new List<ModelDiff>());
        }

        public bool RequireComapre(object lhs, object rhs, out bool equals)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    equals = true;
                    return false;
                }
                else
                {
                    equals = false;
                    List.Add(new ModelDiff
                    {
                        Context = Path,
                        Message = "lhs is null"
                    });
                    return false;
                }
            }
            else
            {
                if (rhs is null)
                {
                    equals = false;
                    List.Add(new ModelDiff
                    {
                        Context = Path,
                        Message = "rhs is null"
                    });
                    return false;
                }
            }

            equals = false;
            return true;
        }
    }
}
