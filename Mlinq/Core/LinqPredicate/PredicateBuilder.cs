using Mlinq.Common;
using Mlinq.Core.Metadata;
using Mlinq.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.LinqPredicate
{
    public static class PredicateBuilder
    {
        private static readonly ConstantPredicate _boolTrue = Constant(true);
        private static readonly ConstantPredicate _boolFalse = Constant(false);
        private static readonly TypeUsage _booleanType = TypeUsage.Create(EdmConstants.GetPrimitiveType(PrimitiveTypeKind.Boolean));

        public static ConstantPredicate True
        {
            get { return _boolTrue; }
        }

        public static ConstantPredicate False
        {
            get { return _boolFalse; }
        }

        public static PredicateBinding BindAs(this Predicate input, string varName)
        {
            Check.NotNull(input, "input");
            Check.NotNull(varName, "varName");
            Check.NotEmpty(varName, "varName");

            CollectionType collectionType = input.ResultType.EdmType as CollectionType;
            if (collectionType == null)
            {
                throw new Exception();
            }
            var inputRef = new VariableReferencePredicate(collectionType.TypeUsage, varName);
            return new PredicateBinding(input, inputRef);
        }

        public static FilterPredicate Filter(this PredicateBinding input, Predicate predicate)
        {
            Check.NotNull(input, "input");
            Check.NotNull(predicate, "predicate");

            var resultType = input.Predicate.ResultType;
            return new FilterPredicate(resultType, input, predicate);
        }

        public static ScanPredicate Scan(this EntitySet targetSet)
        {
            Check.NotNull(targetSet, "targetSet");

            TypeUsage resultType = TypeUsage.Create(new CollectionType(targetSet.ElementType));
            return new ScanPredicate(resultType, targetSet);
        }

        public static ProjectPredicate Project(this PredicateBinding input, Predicate projection)
        {
            Check.NotNull(projection, "projection");
            Check.NotNull(input, "input");

            TypeUsage resultType = TypeUsage.Create(new CollectionType(projection.ResultType));
            return new ProjectPredicate(resultType, input, projection);
        }

        public static PropertyPredicate Property(this Predicate instance, EdmProperty propertyMetadata)
        {
            Check.NotNull(instance, "instance");
            Check.NotNull(propertyMetadata, "propertyMetadata");

            return PropertyFromMember(instance, propertyMetadata, "propertyMetadata");
        }

        public static PropertyPredicate Property(this Predicate instance, string propertyName)
        {
            return PropertyByName(instance, propertyName, false);
        }

        private static PropertyPredicate PropertyByName(Predicate instance, string propertyName, bool ignoreCase)
        {
            Check.NotNull(instance, "instance");
            Check.NotNull(propertyName, "propertyName");
            EntityType entityType = instance.ResultType.EdmType as EntityType;
            EdmProperty property = entityType.Properties.FirstOrDefault(o => o.Name == propertyName);
            return new PropertyPredicate(property.TypeUsage, property, instance);
        }

        private static PropertyPredicate PropertyFromMember(Predicate instance, EdmProperty property, string propertyArgumentName)
        {
            return new PropertyPredicate(property.TypeUsage, property, instance);
        }

        public static ConstantPredicate Constant(object value)
        {
            Check.NotNull(value, "value");

            Type type = value.GetType();
            PrimitiveTypeKind primitiveTypeKind;
            if (!EdmConstants.TryGetPrimitiveTypeKind(type, out primitiveTypeKind))
            {
                throw new ArgumentException("type");
            }

            PrimitiveType primitiveType = EdmConstants.GetPrimitiveType(primitiveTypeKind);
            TypeUsage typeusage = TypeUsage.Create(primitiveType);
            return new ConstantPredicate(typeusage, value);
        }

        public static IsNullPredicate IsNull(this Predicate argument)
        {
            Check.NotNull(argument, "argument");

            return new IsNullPredicate(_booleanType, argument);
        }

        public static ComparisonPredicate Equal(this Predicate left, Predicate right)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");

            return CreateComparison(PredicateType.Equals, left, right);
        }

        private static ComparisonPredicate CreateComparison(PredicateType kind, Predicate left, Predicate right)
        {
            return new ComparisonPredicate(kind, _booleanType, left, right);
        }

        public static AndPredicate And(this Predicate left, Predicate right)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");
            throw new Exception();
        }

        public static NotPredicate Not(this Predicate argument)
        {
            Check.NotNull(argument, "argument");

            var resultType = argument.ResultType;
            return new NotPredicate(resultType, argument);
        }

        public static OrPredicate Or(this Predicate left, Predicate right)
        {
            throw new Exception();
        }

        public static NewInstancePredicate New(this TypeUsage instanceType, IEnumerable<Predicate> arguments)
        {
            Check.NotNull(instanceType, "instanceType");

            return NewInstance(instanceType, arguments);
        }

        private static NewInstancePredicate NewInstance(TypeUsage instanceType, IEnumerable<Predicate> arguments)
        {
            PredicateList validArguments;
            var expectedTypes = GetStructuralMemberTypes(instanceType);
            var pos = 0;
            bool allowEmpty = false;
            var validatedElements = new List<Predicate>();
            int expectedElementCount = expectedTypes.Count;
            bool checkCount = (expectedElementCount != -1);
            var checkNull = (default(Predicate) == null);
            foreach (var elementIn in arguments)
            {
                if (checkCount && pos == expectedElementCount)
                {
                    throw new ArgumentException();
                }

                if (checkNull && elementIn == null)
                {
                    throw new ArgumentNullException();
                }
                validatedElements.Add(elementIn);
                pos++;
            }
            if (checkCount)
            {
                if (pos != expectedElementCount)
                {
                    throw new ArgumentException();
                }
            }
            else
            {
                if (0 == pos && !allowEmpty)
                {
                    throw new ArgumentException();
                }
            }
            validArguments = new PredicateList(validatedElements);
            return new NewInstancePredicate(instanceType, validArguments);
        }

        private static List<TypeUsage> GetStructuralMemberTypes(TypeUsage instanceType)
        {
            var structType = instanceType.EdmType as RowType;
            if (null == structType)
            {
                throw new ArgumentException("instanceType");
            }

            if (structType.Abstract)
            {
                throw new ArgumentException("instanceType");
            }

            var members = structType.Properties;
            if (members == null || members.Count < 1)
            {
                throw new ArgumentException("instanceType");
            }

            var memberTypes = new List<TypeUsage>(members.Count);
            for (var idx = 0; idx < members.Count; idx++)
            {
                memberTypes.Add(members[idx].TypeUsage);
            }
            return memberTypes;
        }
    }
}
