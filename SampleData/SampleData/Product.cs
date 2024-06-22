using Alachisoft.NCache.Runtime.Caching;
using System;
using System.Text;

// ===============================================================================
// Alachisoft (R) NCache Sample Code
// ===============================================================================
// Copyright © Alachisoft.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
// ===============================================================================

namespace Alachisoft.NCache.Sample.Data
{
    /// <summary>
    /// Model class for products
    /// </summary>
    /// 
    [QueryIndexable]
    [Serializable]
    public class Product
    {
        /// <summary>
        ///  Below private fields are not linked with rpoperties to verify if they are being serialized or not 
        /// </summary>
        
       // [JsonProperty]
        private string[] _manufacturers;
        // [JsonProperty]
        private Image[] _images;
        // [JsonProperty]
        private bool _expirable;
       // [JsonProperty]
        private int _id;
       // [JsonProperty]
        private decimal _unitPrice;
        //[JsonProperty]
        private string _name;
        //[JsonProperty]
        private string _className;
       // [JsonProperty]
        private string _category;
        //[JsonProperty]
        private string _quantityPerUnit;
       // [JsonProperty]
        private int _unitsAvailable;
        //[JsonProperty]
        //private DateTime _time;
        //[JsonProperty]
        private Customer _customer;
        //[JsonProperty]
        private Order _order;

        private string _productName;

        public string ProductName { get => _productName; set => _productName = value; }

        //public string ProductName3
        //{
        //    set
        //    {
        //        _productName = value;
        //    }
        //}


        public Product() { }

        public string testField = null;

        public Product(bool expirable) { _expirable = expirable; testField = nameof(expirable); }

        public string[] Manufacturers {
            set;
            get;
        }

        public Image[] Images { get; set; }

        /// <summary>
        /// Tells if Product is Expirable or not
        /// </summary>
        public Boolean Expirable
        {
            set;
            get;
        }

        /// <summary>
        /// Unique id assigned to each product
        /// </summary>
        /// 
        public virtual int Id
        {
            set;
            get;
        }

        /// <summary>
        /// Price of one unit of this product
        /// </summary>
        public Decimal UnitPrice
        {
            get;
            set;
        }

        /// <summary>
        /// Name of the product
        /// </summary>
        public virtual string Name
        {
            set
            {
                _name = value;
            }
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Class to which the product belongs
        /// </summary>
        public virtual string ClassName
        {
            set;
            get;
        }

        /// <summary>
        /// Category of the product
        /// </summary>
        public virtual string Category
        {
            set;
            get;
        }

        /// <summary>
        /// Quantity per unit of the product
        /// </summary>
        public string QuantityPerUnit
        {
            get;
            set;
        }

        /// <summary>
        /// Unit in stock of the product
        /// </summary>
        public int UnitsAvailable
        {
            get;
            set;
        }
        public DateTime Time { get; set; }
        public Customer Customer { get; set; }
        public Order Order { get; set; }
        public object ProductID { get; set; }

        /// <summary>
        /// This method returns the information about this product in string format.
        /// </summary>
        /// <returns> Returns the information about this product. </returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("[");

            builder.Append("ID ");
            builder.Append(Id);

            builder.Append("Expirable ");
            builder.Append(Expirable);

            builder.Append("Name ");
            builder.Append(Name);

            builder.Append(", Quantity/Unit ");
            builder.Append(QuantityPerUnit);

            builder.Append(", UnitPrice ");
            builder.Append(UnitPrice);

            builder.Append(", UnitsAvailable ");
            builder.Append(UnitsAvailable);

            builder.Append("]");

            return builder.ToString();
        }

        public void SetPrivateUnitsAvailable(int unitsAvailable)
        {
            this._unitsAvailable = unitsAvailable;
        }

        /// <summary>
        /// This method sets value for all private fields
        /// </summary>        
        public void SetFields(string[] manufacturers, bool expirable, int id, decimal unitPrice, string name, string className, string category, string quantityPerUnit, int unitsAvailable, DateTime time, Customer customer, Order order)
        {
            _manufacturers = manufacturers;
            _expirable = expirable;
            _id = id;
            _unitPrice = unitPrice;
            _name = name;
            _className = className;
            _category = category;
            _quantityPerUnit = quantityPerUnit;
            _unitsAvailable = unitsAvailable;
            //_time = time;
            _customer = customer;
            _order = order;
        }

        /// <summary>
        /// This metod assigns value of all properties to specific fields.  
        /// </summary>
        /// <remarks>
        /// Properties are not linked with fields
        /// </remarks>     
        public void AssignpropertiesToFields()
        {
            _manufacturers = Manufacturers;
            _expirable = Expirable;
            _id = Id;
            _unitPrice = UnitPrice;
            _name = Name;
            _className = ClassName;
            _category = Category;
            _quantityPerUnit = QuantityPerUnit;
            _unitsAvailable = UnitsAvailable;
            //_time = Time;
            _customer = Customer;
            _order = Order;
        }


    }
}