

namespace SwarmingFleet.Common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class Enumeration
    {
        public class Field<TEnum> : Field<TEnum, Attribute> 
            where TEnum : Enum
        {
            public Field(TEnum field, IEnumerable<Attribute> attributes) : base(field, attributes)
            {
            }

            public Field<TEnum, TAttribute> ExplicitCast<TAttribute>() where TAttribute : Attribute
            {
                return new Field<TEnum, TAttribute>(this.Value, this.OfType<TAttribute>());
            }
        }

        public class Field<TEnum, TAttribute> : IReadOnlyList<TAttribute> 
            where TEnum : Enum 
            where TAttribute : Attribute
        {
            public Field(TEnum field, IEnumerable<TAttribute> attributes)
            {
                this.Value = field;
                this._attributes = attributes?.ToArray() ?? Array.Empty<TAttribute>();
            }

            public readonly TEnum Value;
            private readonly IReadOnlyList<TAttribute> _attributes;

            public TAttribute this[int index] => this._attributes[index];

            public int Count => this._attributes.Count;

            public IEnumerator<TAttribute> GetEnumerator()
            {
                return this._attributes.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this._attributes.GetEnumerator();
            }

        }



        public static IReadOnlyList<Field<TEnum>> AllMetadata<TEnum>()
           where TEnum : Enum
        {
            return EnumAttributeCache<TEnum>.Fields;
        }

        private delegate IOrderedEnumerable<TSource> Order<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector);

        public static IEnumerable<Field<TEnum>> Metadata<TEnum>(this TEnum @enum, bool descending = true)
           where TEnum : Enum
        {
            var k = @enum; 
            foreach (var field in (descending ? (Order<Field<TEnum>, TEnum>)Enumerable.OrderByDescending : Enumerable.OrderBy)(EnumAttributeCache<TEnum>.Fields, x => x.Value))
            {
                if (k.Has(field.Value))
                {
                    yield return field;
                    k = k.Detach(field.Value);
                }
            } 
        }


        public static TEnum Attach<TEnum>(this TEnum source, TEnum value) where TEnum : Enum
        {
            return EnumExpressionBuilder<TEnum>.AttachDelegate(source, value);
        }

        public static TEnum Detach<TEnum>(this TEnum source, TEnum value) where TEnum : Enum
        {
            return EnumExpressionBuilder<TEnum>.DetachDelegate(source, value);
        }
        public static bool Has<TEnum>(this TEnum source, TEnum value) where TEnum : Enum
        {
            return EnumExpressionBuilder<TEnum>.HasDelegate(source, value);
        }


        internal static class EnumAttributeCache<TEnum> where TEnum : Enum
        {
            internal static readonly IImmutableList<Field<TEnum>> Fields;

            static EnumAttributeCache()
            {
                var t = typeof(TEnum);
                var builder = ImmutableArray.CreateBuilder<Field<TEnum>>();

                var values = Enum.GetValues(t) as TEnum[];
                foreach (TEnum item in values)
                {
                    var field = t.GetField(item.ToString());
                    builder.Add(new Field<TEnum>(item, field.GetCustomAttributes()));
                }
                Fields = builder.ToImmutable();
            }
        }

        internal static class EnumExpressionBuilder<TEnum> where TEnum : Enum
        {
            static EnumExpressionBuilder()
            {
                var t = typeof(TEnum);
                var underly = Enum.GetUnderlyingType(t);

                var src = Expression.Parameter(t);
                var val = Expression.Parameter(t);

                var castedSrc = Expression.Convert(src, underly);
                var castedVal = Expression.Convert(val, underly);

                var has = Expression.Equal(Expression.And(castedSrc, castedVal), castedVal);
                HasDelegate = Expression.Lambda<Func<TEnum, TEnum, bool>>(has, src, val).Compile();

                var attach = Expression.Convert(Expression.Or(castedSrc, castedVal), t);
                AttachDelegate = Expression.Lambda<Func<TEnum, TEnum, TEnum>>(attach, src, val).Compile();

                var detech = Expression.Convert(Expression.And(castedSrc, Expression.OnesComplement(castedVal)), t);
                DetachDelegate = Expression.Lambda<Func<TEnum, TEnum, TEnum>>(detech, src, val).Compile();
                                
            }

            internal readonly static Func<TEnum, TEnum, bool> HasDelegate;
            internal readonly static Func<TEnum, TEnum, TEnum> AttachDelegate;
            internal readonly static Func<TEnum, TEnum, TEnum> DetachDelegate;

        }
    }
     

    
}
