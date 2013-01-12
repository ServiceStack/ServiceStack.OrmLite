using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ServiceStack.OrmLite
{
    /// <summary>
    /// Used to pass in extra Data Annotation Attributes when creating a database table from a class.
    /// The key is the property or class name and the value is a collection of Attributes
    /// which should apply.
    /// </summary>
    public class DataAnnotationsCollection : Dictionary<string, IEnumerable<Attribute>>
    {
        /// <summary>
        /// Add a reference to a property or class and DataAnnotation Attributes to apply to it
        /// </summary>
        /// <param name="dataElementName">Name of the Property or Class which you wish to decorate.</param>
        /// <param name="dataAnnotationAttributes">DataAnnotation Attributes which should apply to the given Property or Class</param>
        public new void Add(string dataElementName, IEnumerable<Attribute> dataAnnotationAttributes)
        {
            //We could throw an ArgumentException here if the Attribute is not an expected
            //data annotation attribute, but it would add overhead, and currently the only consequence 
            //of having irrelevant Attributes is that they would never be looked up
            base.Add(dataElementName, dataAnnotationAttributes);
        }

        /// <summary>
        /// Add a reference to a property or class and a single DataAnnotation Attribute to apply to it
        /// </summary>
        /// <param name="dataElementName">The Name of a Property or Class which you wish to decorate.</param>
        /// <param name="dataAnnotationAttribute">A single DataAnnotation Attribute which 
        /// should apply to the given Property or Class</param>
        public void Add(string dataElementName, Attribute dataAnnotationAttribute)
        {
            Add(dataElementName, new[] { dataAnnotationAttribute });
        }

        /// <summary>
        /// Add Data Annotation Attributes against a strongly-typed Property reference using a lambda expression,
        /// E.g. AddProperty(() => new MyModel().MyModelId, new[] { new AutoIncrementAttribute() })
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="propertyLambda">lambda expression of the form: '() => Class.Property' or '() => object.Property'</param>
        /// <param name="dataAnnotationAttributes">Data Annotation Attributes which should apply when 
        /// creating a database column from the specified property</param>
        public void Add<T>(Expression<Func<T>> propertyLambda, IEnumerable<Attribute> dataAnnotationAttributes)
        {
            Add(GetPropertyName(propertyLambda), dataAnnotationAttributes);
        }

        /// <summary>
        /// Add a single Data Annotation Attribute against a strongly-typed Property reference using a lambda expression,
        /// E.g. AddProperty(() => new MyModel().MyModelId, new AutoIncrementAttribute())
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="propertyLambda">lambda expression of the form: '() => Class.Property' or '() => object.Property'</param>
        /// <param name="dataAnnotationAttribute">A single Data Annotation Attribute which should apply when 
        /// creating a database column from the specified property</param>
        public void Add<T>(Expression<Func<T>> propertyLambda, Attribute dataAnnotationAttribute)
        {
            Add(propertyLambda, new[] { dataAnnotationAttribute });
        }

        /// <summary>
        /// Add Data Annotation Attributes against a strongly-typed Class Type.
        /// </summary>
        /// <param name="classType">The Type of the model Class</typeparam>
        /// <param name="dataAnnotationAttributes">Data Annotation Attributes which should apply when 
        /// creating a database table from Class <paramref name="classType"/></param></param>
        public void Add(Type classType, IEnumerable<Attribute> dataAnnotationAttributes)
        {
            Add(classType.Name, dataAnnotationAttributes);
        }

        /// <summary>
        /// Add a single Data Annotation Attribute against a strongly-typed Class Type.
        /// </summary>
        /// <param name="classType">The Type of the model Class</typeparam>
        /// <param name="dataAnnotationAttribute">A single Data Annotation Attribute which should apply when 
        /// creating a database table from Class <paramref name="classType"/></param>
        public void Add(Type classType, Attribute dataAnnotationAttribute)
        {
            Add(classType, new[] { dataAnnotationAttribute });
        }


        /// <summary>
        /// Find an Attribute based on a Property or Class Name
        /// </summary>
        /// <typeparam name="TAttribute">The type of attribute to look for</typeparam>
        /// <param name="dataElementName">A Property or Class Name</param>
        /// <returns>The first Attribute found of the specified Type, or null (default(TAttribute))</returns>
        internal TAttribute FindAnnotation<TAttribute>(string dataElementName)
        {
            if (ContainsKey(dataElementName))
            {
                //establish Type once before searching, so it is not done on every pass
                var requiredAttributeType = typeof(TAttribute);
                return (TAttribute)(object)this[dataElementName]
                                                    .FirstOrDefault(
                                                        att => att.GetType().Equals(requiredAttributeType));
            }
            else
            {
                return default(TAttribute);
            }
        }


        /// <summary>
        /// Get the name of a static or instance property from a property access lambda.
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="propertyLambda">lambda expression of the form: '() => Class.Property' or '() => object.Property'</param>
        private static string GetPropertyName<T>(Expression<Func<T>> propertyLambda)
        {
            var memberExpression = propertyLambda.Body as MemberExpression;

            if (memberExpression == null)
            {
                throw new ArgumentException("propertyLambda must be a lambda of the form: '() => Class.Property' or '() => object.Property'");
            }

            return memberExpression.Member.Name;
        }
    }
}
