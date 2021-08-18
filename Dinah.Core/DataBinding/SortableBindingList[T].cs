﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

namespace Dinah.Core.DataBinding
{
	public class SortableBindingList<T> : BindingList<T> where T : IMemberComparable
	{
		private bool isSorted;
		private ListSortDirection listSortDirection;
		private PropertyDescriptor propertyDescriptor;

		public SortableBindingList() : base(new List<T>()) { }
		public SortableBindingList(IEnumerable<T> enumeration) : base(new List<T>(enumeration)) { }

		private MemberComparer<T> Comparer { get; } = new();
		protected override bool SupportsSortingCore => true;
		protected override bool SupportsSearchingCore => true;
		protected override bool IsSortedCore => isSorted;
		protected override PropertyDescriptor SortPropertyCore => propertyDescriptor;
		protected override ListSortDirection SortDirectionCore => listSortDirection;

		protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
		{
			List<T> itemsList = (List<T>)Items;

			Comparer.PropertyName = property.Name;
			Comparer.Direction = direction;

			//Array.Sort() and List<T>.Sort() are unstable sorts. OrderBy is stable.
			var sortedItems = itemsList.OrderBy((ge) => ge, Comparer).ToList();

			itemsList.Clear();
			itemsList.AddRange(sortedItems);

			propertyDescriptor = property;
			listSortDirection = direction;
			isSorted = true;

			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}

		protected override void RemoveSortCore()
		{
			isSorted = false;
			propertyDescriptor = base.SortPropertyCore;
			listSortDirection = base.SortDirectionCore;

			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}

		protected override int FindCore(PropertyDescriptor property, object key)
		{
			int count = Count;

			System.Collections.IComparer valueComparer = null;

			for (int i = 0; i < count; ++i)
			{
				T element = this[i];
				var elemValue = element.GetMemberValue(property.Name);
				valueComparer ??= element.GetMemberComparer(elemValue.GetType());

				if (valueComparer.Compare(elemValue, key) == 0)
				{
					return i;
				}
			}

			return -1;
		}
	}
}
