﻿using System;
using System.Collections.Generic;
using System.Text;

namespace GraphQlClientGenerator
{
    public delegate string GetCustomScalarFieldTypeDelegate(GraphQlType baseType, GraphQlTypeBase valueType, string valueName);
    
    public delegate string GetDataPropertyAccessorBodiesDelegate(string backingFieldName, string backingFieldType);

    public static class GraphQlGeneratorConfiguration
    {
        public static CSharpVersion CSharpVersion { get; set; }

        public static string ClassPostfix { get; set; }

        public static IDictionary<string, string> CustomClassNameMapping { get; } = new Dictionary<string, string>();

        public static CommentGenerationOption CommentGeneration { get; set; }

        public static bool IncludeDeprecatedFields { get; set; }

        public static bool GeneratePartialClasses { get; set; } = true;

        /// <summary>
        /// Determines whether unknown type scalar fields will be automatically requested when <code>WithAllScalarFields</code> issued.
        /// </summary>
        public static bool TreatUnknownObjectAsScalar { get; set; }

        public static IntegerTypeMapping IntegerTypeMapping { get; set; } = IntegerTypeMapping.Int32;

        public static FloatTypeMapping FloatTypeMapping { get; set; }

        public static BooleanTypeMapping BooleanTypeMapping { get; set; }

        public static IdTypeMapping IdTypeMapping { get; set; } = IdTypeMapping.Guid;
        
        public static PropertyGenerationOption PropertyGeneration { get; set; } = PropertyGenerationOption.AutoProperty;

        public static JsonPropertyGenerationOption JsonPropertyGeneration { get; set; } = JsonPropertyGenerationOption.CaseInsensitive;

        /// <summary>
        /// Determines builder class, data class and interfaces accessibility level.
        /// </summary>
        public static MemberAccessibility MemberAccessibility { get; set; }

        /// <summary>
        /// This property is used for mapping GraphQL scalar type into specific .NET type. By default any custom GraphQL scalar type is mapped into <see cref="System.Object"/>.
        /// </summary>
        public static GetCustomScalarFieldTypeDelegate CustomScalarFieldTypeMapping { get; set; } = DefaultScalarFieldTypeMapping;

        /// <summary>
        /// Used for custom data property accessor bodies generation; applicable only when <code>PropertyGeneration = PropertyGenerationOption.BackingField</code>.
        /// </summary>
        public static GetDataPropertyAccessorBodiesDelegate PropertyAccessorBodyWriter { get; set; } = GeneratePropertyAccessors;

        public static void Reset()
        {
            ClassPostfix = null;
            CustomClassNameMapping.Clear();
            CSharpVersion = CSharpVersion.Compatible;
            CustomScalarFieldTypeMapping = DefaultScalarFieldTypeMapping;
            PropertyAccessorBodyWriter = GeneratePropertyAccessors;
            CommentGeneration = CommentGenerationOption.Disabled;
            IncludeDeprecatedFields = false;
            FloatTypeMapping = FloatTypeMapping.Decimal;
            BooleanTypeMapping = BooleanTypeMapping.Boolean;
            IntegerTypeMapping = IntegerTypeMapping.Int32;
            IdTypeMapping = IdTypeMapping.Guid;
            TreatUnknownObjectAsScalar = false;
            GeneratePartialClasses = true;
            MemberAccessibility = MemberAccessibility.Public;
            JsonPropertyGeneration = JsonPropertyGenerationOption.CaseInsensitive;
            PropertyGeneration = PropertyGenerationOption.AutoProperty;
        }

        public static string DefaultScalarFieldTypeMapping(GraphQlType baseType, GraphQlTypeBase valueType, string valueName)
        {
            valueName = NamingHelper.ToPascalCase(valueName);
            if (valueName == "From" || valueName == "ValidFrom" || valueName == "To" || valueName == "ValidTo" ||
                valueName == "CreatedAt" || valueName == "UpdatedAt" || valueName == "ModifiedAt" || valueName == "DeletedAt" ||
                valueName.EndsWith("Timestamp"))
                return "DateTimeOffset?";

            valueType = (valueType as GraphQlFieldType)?.UnwrapIfNonNull() ?? valueType;
            if (valueType.Kind == GraphQlTypeKind.Enum)
                return valueType.Name + "?";

            var dataType = valueType.Name == GraphQlTypeBase.GraphQlTypeScalarString ? "string" : "object";
            return GraphQlGenerator.AddQuestionMarkIfNullableReferencesEnabled(dataType);
        }

        public static string GeneratePropertyAccessors(string backingFieldName, string backingFieldType)
        {
            var useCompatibleVersion = CSharpVersion == CSharpVersion.Compatible;
            var builder = new StringBuilder();
            builder.Append(" { get");
            builder.Append(useCompatibleVersion ? " { return " : " => ");
            builder.Append(backingFieldName);
            builder.Append(";");

            if (useCompatibleVersion)
                builder.Append(" }");

            builder.Append(" set");
            builder.Append(useCompatibleVersion ? " { " : " => ");
            builder.Append(backingFieldName);
            builder.Append(" = value;");

            if (useCompatibleVersion)
                builder.Append(" }");

            builder.Append(" }");

            return builder.ToString();
        }
    }

    public enum CSharpVersion
    {
        Compatible,
        Newest,
        NewestWithNullableReferences
    }

    public enum FloatTypeMapping
    {
        Decimal,
        Float,
        Double,
        Custom
    }

    public enum BooleanTypeMapping
    {
        Boolean,
        Custom
    }

    public enum IntegerTypeMapping
    {
        Int16,
        Int32,
        Int64,
        Custom
    }

    public enum IdTypeMapping
    {
        String,
        Guid,
        Object,
        Custom
    }

    public enum MemberAccessibility
    {
        Public,
        Internal
    }

    public enum JsonPropertyGenerationOption
    {
        Never,
        Always,
        CaseInsensitive,
        CaseSensitive
    }

    public enum PropertyGenerationOption
    {
        AutoProperty,
        BackingField
    }

    [Flags]
    public enum CommentGenerationOption
    {
        Disabled = 0,
        CodeSummary = 1,
        DescriptionAttribute = 2
    }
}