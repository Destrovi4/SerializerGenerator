#define PRINT_TO_FILE
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Destr.Codegen.Source
{
    public class SourceGenerator
    {
        public const string Space = "    ";

        private readonly HashSet<object> _dependence = new HashSet<object>();

        private List<object> _sources = new List<object>();

        public virtual void Clear()
        {
            _dependence.Clear();
            _sources.Clear();
        }

        public SourceGenerator Add(string src)
        {
            _sources.Add(src);
            return this;
        }

        public SourceGenerator Add(IEnumerable<string> src) 
        {
            _sources.Add(src);
            return this;
        }

        public SourceGenerator Add(Func<string> src)
        {
            _sources.Add(src);
            return this;
        }

        public SourceGenerator Add(Func<IEnumerable<string>> src)
        {
            _sources.Add(src);
            return this;
        }

        public SourceGenerator Add(SourceGenerator src)
        {
            Require(src);
            _sources.Add(src);
            return this;
        }

        public SourceGenerator Add(Type type)
        {
            Require(type);
            _sources.Add(RealTypeName(type));
            return this;
        }

        public SourceGenerator Add(Type type, params Type[] args)
        {
            Require(type);
            Require(args);
            if(args == null)
                _sources.Add($"{SimpleName(type)}");
            else
                _sources.Add($"{SimpleName(type)}<{string.Join(",", args.Select(RealTypeName))}>");
            return this;
        }

        public SourceGenerator Add<T>()
        {
            Add(typeof(T));
            return this;
        }

        public LineSourceGenerator AddLine()
        {
            LineSourceGenerator generator = new LineSourceGenerator();
            Add(generator);
            return generator;
        }

        public LineSourceGenerator AddLine(string line)
        {
            LineSourceGenerator generator = AddLine();
            generator.Add(line);
            return generator;
        }

        public LineSourceGenerator Line => AddLine();

        public SourceGenerator Require<T>()
        {
            _dependence.Add(typeof(T));
            return this;
        }

        public SourceGenerator Require(Type type)
        {
            _dependence.Add(type);
            return this;
        }
        
        public SourceGenerator Require(IEnumerable<Type> type)
        {
            _dependence.Add(type);
            return this;
        }

        public SourceGenerator Require(Func<IEnumerable<Type>> type)
        {
            _dependence.Add(type);
            return this;
        }

        public SourceGenerator Require(SourceGenerator type)
        {
            _dependence.Add(type);
            return this;
        }

        public IEnumerable<Type> Dependence()
        {
            foreach (var type in ExtractTypes(_dependence))
                yield return type;
        }

        public virtual IEnumerable<string> GetSourceLines() {
            foreach(var line in ExtractString(_sources))
                yield return line;
        }

        protected IEnumerable<T> Extract<T>(object obj, Func<object, IEnumerable<T>> other = null)
        {
            //Console.WriteLine(obj);
            if (obj is T)
            {
                yield return (T)obj;
                yield break;
            }

            if (obj is Func<object>)
            {
                foreach (var t in Extract<T>((obj as Func<object>)(), other))
                    yield return t;
                yield break;
            }
            /*
            if (obj is List<object> objs)
            {
                foreach (var o in objs)
                    foreach (var t in Extract<T>(o, other))
                        yield return t;
                yield break;
            }
            */
            if (obj is IEnumerable<T>)
            {
                foreach (var t in obj as IEnumerable<T>)
                    yield return t;
                yield break;
            }

            if (obj is IEnumerable<object>)
            {
                foreach (var o in (obj as IEnumerable<object>).ToArray())
                    foreach (var t in Extract<T>(o, other))
                        yield return t;
                yield break;
            }

            if(other != null)
                foreach(var t in other(obj))
                    yield return t;
        }

        protected IEnumerable<string> ExtractString(object obj) => Extract(obj, OtherStringSources);

        protected IEnumerable<string> OtherStringSources(object obj)
        {
            if (obj is SourceGenerator)
                foreach (string line in (obj as SourceGenerator).GetSourceLines())
                    yield return line;
            yield break;
        }

        protected IEnumerable<Type> ExtractTypes(object obj) => Extract(obj, OtherTypeSources);

        protected IEnumerable<Type> OtherTypeSources(object obj)
        {
            if (obj is SourceGenerator)
                foreach (Type type in (obj as SourceGenerator).Dependence())
                    yield return type;
            yield break;
        }

        internal static string RealTypeName(Type type)
        {
            var name = type.Name;
            if (!type.IsGenericType) return name;
            var sb = new StringBuilder();
            sb.Append(name.Substring(0, name.IndexOf('`')));
            sb.Append("<");
            sb.Append(string.Join(", ", type.GetGenericArguments().Select(t => RealTypeName(t))));
            sb.Append(">");
            return sb.ToString();
        }

        protected static string FieldName(Type type)
        {
            return type.FullName.Replace('`', '_').Replace('.', '_');
        }

        public static string SimpleName(Type type)
        {
            var name = type.Name;
            if (!type.IsGenericType) return name;
            return name.Substring(0, name.IndexOf('`'));
        }


        public void Write(string path)
        {
#if PRINT_TO_FILE
            using (var writer = new StreamWriter(path))
                Write(writer);
#else
            Console.Out.WriteLine($"{nameof(path)}: {path}");
            Write(Console.Out);
#endif
        }


        public void Write(TextWriter writer)
        {
            foreach (var line in GetSourceLines())
                writer.WriteLine(line);
        }
    }
}
