// ===============================================================================
// Alachisoft (R) NCache Sample Code
// ===============================================================================
// Copyright © Alachisoft.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
// ===============================================================================

using System;
namespace Alachisoft.NCache.Sample.Data
{
    /// <summary>
    /// Model class for orders
    /// </summary>
    [Serializable]
    public class Order
    {
        /// <summary>
        /// Unique Id assigned to each order
        /// </summary>
        public int OrderID
        {
            set;
            get;
        }

        /// <summary>
        /// The time when order was made
        /// </summary>
        public DateTime OrderDate
        {
            set;
            get;
        }

        /// <summary>
        /// Name of the person whom order is to be delivered
        /// </summary>
        public string ShipName
        {
            set;
            get;
        }

        /// <summary>
        /// The address where order is to be delivered
        /// </summary>
        public string ShipAddress
        {
            set;
            get;
        }

        /// <summary>
        /// City where order is to be delivered
        /// </summary>
        public string ShipCity
        {
            set;
            get;
        }

        /// <summary>
        /// Country where order is to be delivered
        /// </summary>
        public string ShipCountry
        {
            set;
            get;
        }
        public Product Product { get; set; }

        public override bool Equals(object obje)
        {
            Order obj = obje as Order;

            if (obj == null)
                return false;

            if (obj.OrderDate != this.OrderDate)
                return false;

            if (obj.OrderID != this.OrderID)
                return false;

            if (obj.ShipName != this.ShipName)
                return false;

            if (obj.ShipCity != this.ShipCity)
                return false;

            if (obj.ShipAddress != this.ShipAddress)
                return false;

            if (obj.ShipCountry != this.ShipCountry)
                return false;

            if ((obj.Product == null && this.Product != null) || (obj.Product != null && this.Product == null))
                return false;

            if (obj.Product != null && this.Product != null)
            {
                if (obj.Product.Id != this.Product.Id)
                    return false;
            }

           

            return true;
        }

    }
}
