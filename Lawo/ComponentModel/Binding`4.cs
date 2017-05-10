////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.ComponentModel
{
    using System;
    using System.ComponentModel;
    using Reflection;

    /// <summary>Represents a binding between two properties.</summary>
    /// <typeparam name="TSourceOwner">The type of the object owning the source property.</typeparam>
    /// <typeparam name="TSource">The type of the source property.</typeparam>
    /// <typeparam name="TTargetOwner">The type of the object owning the target property.</typeparam>
    /// <typeparam name="TTarget">The type of the target property.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public sealed class Binding<TSourceOwner, TSource, TTargetOwner, TTarget> : IDisposable
        where TSourceOwner : INotifyPropertyChanged
        where TTargetOwner : INotifyPropertyChanged
    {
        /// <summary>Occurs when a change has originated at the source.</summary>
        public event EventHandler<ChangeOriginatedAtEventArgs<TSourceOwner, TSource>> ChangeOriginatedAtSource;

        /// <summary>Occurs when a change has originated at the target.</summary>
        public event EventHandler<ChangeOriginatedAtEventArgs<TTargetOwner, TTarget>> ChangeOriginatedAtTarget;

        /// <summary>Gets the source property.</summary>
        public IProperty<TSourceOwner, TSource> Source => this.sourceEventArgs.Property;

        /// <summary>Gets the target property.</summary>
        public IProperty<TTargetOwner, TTarget> Target => this.targetEventArgs.Property;

        /// <summary>Stops forwarding changes and altering the properties.</summary>
        /// <remarks>If the binding is intended to be permanent it is permissible to to never call
        /// <see cref="Dispose"/>.</remarks>
        public void Dispose()
        {
            this.sourceEventArgs.Property.Owner.PropertyChanged -= this.OnSourceChanged;
            this.targetEventArgs.Property.Owner.PropertyChanged -= this.OnTargetChanged;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal Binding(
            IProperty<TSourceOwner, TSource> source,
            Func<TSource, TTarget> toTarget,
            IProperty<TTargetOwner, TTarget> target,
            Func<TTarget, TSource> toSource)
        {
            this.sourceEventArgs = new ChangeOriginatedAtEventArgs<TSourceOwner, TSource>(
                source ?? throw new ArgumentNullException(nameof(source)));
            this.toTarget = toTarget ?? throw new ArgumentNullException(nameof(toTarget));
            this.targetEventArgs = new ChangeOriginatedAtEventArgs<TTargetOwner, TTarget>(
                target ?? throw new ArgumentNullException(nameof(target)));
            this.toSource = toSource;

            this.targetEventArgs.Property.Value = this.toTarget(this.sourceEventArgs.Property.Value);
            this.sourceEventArgs.Property.Owner.PropertyChanged += this.OnSourceChanged;

            if (this.toSource != null)
            {
                this.targetEventArgs.Property.Owner.PropertyChanged += this.OnTargetChanged;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly ChangeOriginatedAtEventArgs<TSourceOwner, TSource> sourceEventArgs;
        private readonly Func<TSource, TTarget> toTarget;
        private readonly ChangeOriginatedAtEventArgs<TTargetOwner, TTarget> targetEventArgs;
        private readonly Func<TTarget, TSource> toSource;
        private byte sourceUpdating;
        private byte targetUpdating;

        private void OnSourceChanged(object sender, PropertyChangedEventArgs e) =>
            this.OnChanged(
                e,
                this.sourceEventArgs,
                this.sourceUpdating,
                this.targetEventArgs.Property,
                ref this.targetUpdating,
                this.toTarget,
                this.ChangeOriginatedAtSource);

        private void OnTargetChanged(object sender, PropertyChangedEventArgs e) =>
            this.OnChanged(
                e,
                this.targetEventArgs,
                this.targetUpdating,
                this.sourceEventArgs.Property,
                ref this.sourceUpdating,
                this.toSource,
                this.ChangeOriginatedAtTarget);

        private void OnChanged<TLeaderOwner, TLeader, TFollowerOwner, TFollower>(
            PropertyChangedEventArgs e,
            ChangeOriginatedAtEventArgs<TLeaderOwner, TLeader> leaderEventArgs,
            byte leaderUpdating,
            IProperty<TFollowerOwner, TFollower> follower,
            ref byte followerUpdating,
            Func<TLeader, TFollower> toFollower,
            EventHandler<ChangeOriginatedAtEventArgs<TLeaderOwner, TLeader>> handler)
            where TLeaderOwner : INotifyPropertyChanged
            where TFollowerOwner : INotifyPropertyChanged
        {
            if (e.PropertyName == leaderEventArgs.Property.PropertyInfo.Name)
            {
                ++followerUpdating;

                try
                {
                    follower.Value = toFollower(leaderEventArgs.Property.Value);
                }
                finally
                {
                    --followerUpdating;
                }

                if ((handler != null) && (leaderUpdating == 0))
                {
                    handler(this, leaderEventArgs);
                }
            }
        }
    }
}
