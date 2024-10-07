/*
 * Copyright (c) 2021 PlayEveryWare
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

namespace PlayEveryWare.EpicOnlineServices
{
    using Epic.OnlineServices.Platform;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Used to associate a name with a particular type.
    /// </summary>
    public class Named<T> : IEquatable<Named<T>>, IComparable<Named<T>>, IEquatable<T> where T : IEquatable<T>
    {
        /// <summary>
        /// The name of the value (typically used for things like UI labels)
        /// </summary>
        public string Name;

        /// <summary>
        /// The value itself.
        /// </summary>
        public T Value;

        public static Named<T> FromValue(T value, string name)
        {
            return new Named<T>() { Name = name, Value = value };
        }

        public int CompareTo(Named<T> other)
        {
            return string.Compare(Name, other.Name, StringComparison.Ordinal);
        }

        public bool Equals(T other)
        {
            if (Value == null && other == null)
                return true;

            if (Value == null)
                return false;

            return Value.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return obj is Named<T> other && Equals(other);
        }

        public bool Equals(Named<T> other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Value);
        }

        public override string ToString()
        {
            return $"{Name} : {Value}";
        }
    }

    public class Deployment : IEquatable<Deployment>
    {
        public readonly string SandboxId;

        public Guid DeploymentId;

        public bool Equals(Deployment other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return SandboxId == other.SandboxId && DeploymentId.Equals(other.DeploymentId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((Deployment)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SandboxId, DeploymentId);
        }
    }

    public class SortedSetOfNamed<T> : SortedSet<Named<T>> where T : IEquatable<T>
    {
        public bool Add(string name, T value)
        {
            return !ContainsName(name) && base.Add(Named<T>.FromValue(value, name));
        }

        public bool ContainsName(string name)
        {
            foreach (Named<T> item in this)
            {
                if (item.Name == name)
                {
                    return true;
                }
            }

            return false;
        }

        public bool Contains(T item)
        {
            Named<T> temp = Named<T>.FromValue(item, "temp");
            return Contains(temp);
        }
    }

    public abstract class WrappedStruct<T> : IEquatable<WrappedStruct<T>> where T : struct
    {
        protected T _value;

        protected WrappedStruct(T value)
        {
            _value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is WrappedStruct<T> wrappedStruct && Equals(wrappedStruct);
        }

        public abstract bool Equals(WrappedStruct<T> other);

        public T Unwrap()
        {
            return _value;
        }
    }

    public class WrappedClientCredentials : WrappedStruct<ClientCredentials>
    {
        public WrappedClientCredentials(ClientCredentials credentials) : base(credentials) { }

        public override bool Equals(WrappedStruct<ClientCredentials> other)
        {
            if (other == null)
            {
                return false;
            }

            var temp = other.Unwrap();

            return (temp.ClientId == _value.ClientId && 
                    temp.ClientSecret == _value.ClientSecret);
        }
    }

    /// <summary>
    /// Contains information about the product entered by the user from the Epic
    /// Developer Portal.
    /// </summary>
    public class ProductConfig : Config
    {
        /// <summary>
        /// The product ID is a unique GUID labeled "Product ID" in the Epic
        /// Developer Portal. The name for this value can be set to anything -
        /// it is used as a label for user interface purposes - and is allowed
        /// to differ from the label given to it on the Developer Portal.
        /// </summary>
        public Named<Guid> ProductId;

        /// <summary>
        /// The set of Clients as defined within the Epic Developer Portal. For
        /// EOS to function, at least one of these must be set, and the
        /// platform config needs to indicate which one to use. (If none is
        /// explicitly indicated, and only one is defined, that one will be
        /// used).
        /// </summary>
        public SortedSetOfNamed<WrappedClientCredentials> Clients;

        /// <summary>
        /// The set of Sandboxes as defined within the Epic Developer Portal. For
        /// EOS to function, at least one of these must be set, and it must
        /// match the deployment indicated by the platform config.
        /// </summary>
        public SortedSetOfNamed<string> Sandboxes;

        /// <summary>
        /// Backing field member for the Deployments property.
        /// </summary>
        private SortedSetOfNamed<Deployment> _deployments;

        /// <summary>
        /// The set of Deployments as defined within the Epic Developer Portal.
        /// Every Deployment must point to a Sandbox that 
        /// </summary>
        public SortedSetOfNamed<Deployment> Deployments
        {
            get
            {
                return _deployments;
            }
        }

        public void AddDeployment(string name, Deployment deployment)
        {
            if (!Sandboxes.Contains(deployment.SandboxId))
            {
                Sandboxes.Add(deployment.SandboxId)
            }
        }

        static ProductConfig()
        {
            RegisterFactory(() => new ProductConfig());
        }

        protected ProductConfig() : base("eos_product_config.json") { }
    }
}