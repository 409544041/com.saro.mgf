﻿using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;


namespace Saro.MoonAsset
{
    internal class SharedPoolTree : TreeView
    {
        private List<SharedPoolInfo> m_Infos = new List<SharedPoolInfo>();
        private static Texture2D tex_warn = EditorGUIUtility.FindTexture("console.warnicon.sml");

        internal SharedPoolTree(TreeViewState state, MultiColumnHeaderState mchs) : base(state, new MultiColumnHeader(mchs))
        {
            showBorder = true;
            showAlternatingRowBackgrounds = true;

            multiColumnHeader.canSort = false;
            //multiColumnHeader.sortingChanged += OnSortingChanged;
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };

            SharedPool.GetAllReferencePoolInfos(ref m_Infos);

            for (int i = 0; i < m_Infos.Count; i++)
            {
                SharedPoolInfo info = m_Infos[i];
                root.AddChild(new SharedPoolItem(info));
            }

            return root;
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override bool CanBeParent(TreeViewItem item)
        {
            return false;
        }

        protected override bool CanChangeExpandedState(TreeViewItem item)
        {
            return false;
        }


        #region MyRegion

        internal static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            return new MultiColumnHeaderState(GetColumns());
        }
        private static MultiColumnHeaderState.Column[] GetColumns()
        {
            var retVal = new MultiColumnHeaderState.Column[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Type", ""),
                    minWidth = 250,
                    width = 400,
                    maxWidth = 420,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = true,
                    autoResize = true,
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("AcquireCount", ""),
                    minWidth = 100,
                    width = 100,
                    maxWidth = 100,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = true,
                    autoResize = false,
                 },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("ReleaseCount", ""),
                    minWidth = 100,
                    width = 100,
                    maxWidth = 100,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = true,
                    autoResize = false,
                 },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("UsingCount", ""),
                    minWidth = 100,
                    width = 100,
                    maxWidth = 100,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = true,
                    autoResize = false,
                 },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("UnusingCount", ""),
                    minWidth = 100,
                    width = 100,
                    maxWidth = 100,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = true,
                    autoResize = false,
                 },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("AddCount", ""),
                    minWidth = 100,
                    width = 100,
                    maxWidth = 100,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = true,
                    autoResize = false,
                 },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("RemoveCount", ""),
                    minWidth = 100,
                    width = 100,
                    maxWidth = 100,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = true,
                    autoResize = false,
                 },
            };

            return retVal;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                CellGUI(args.GetCellRect(i), args.item as SharedPoolItem, args.GetColumn(i), ref args);
        }

        private void CellGUI(Rect cellRect, SharedPoolItem item, int column, ref RowGUIArgs args)
        {
            var bundleItem = (args.item as SharedPoolItem);
            if (args.item.icon == null)
                extraSpaceBeforeIconAndLabel = 16f;
            else
                extraSpaceBeforeIconAndLabel = 0f;

            Color old = GUI.color;
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch (column)
            {
                case 0:

                    var iconRect = new Rect(cellRect.x + 1, cellRect.y + 1, cellRect.height - 2, cellRect.height - 2);
                    if (item.info.AcquireCount != item.info.ReleaseCount)
                    {
                        GUI.DrawTexture(iconRect, tex_warn, ScaleMode.ScaleToFit);
                    }
                    DefaultGUI.Label(
                        new Rect(cellRect.x + iconRect.xMax + 1, cellRect.y, cellRect.width - iconRect.width, cellRect.height),
                        item.info.Type.ToString(),
                        args.selected,
                        args.focused);
                    break;
                case 1:
                    DefaultGUI.Label(cellRect, item.info.AcquireCount.ToString(), args.selected, args.focused);
                    break;
                case 2:
                    DefaultGUI.Label(cellRect, item.info.ReleaseCount.ToString(), args.selected, args.focused);
                    break;
                case 3:
                    DefaultGUI.Label(cellRect, item.info.UsingCount.ToString(), args.selected, args.focused);
                    break;
                case 4:
                    DefaultGUI.Label(cellRect, item.info.UnusedCount.ToString(), args.selected, args.focused);
                    break;
                case 5:
                    DefaultGUI.Label(cellRect, item.info.AddCount.ToString(), args.selected, args.focused);
                    break;
                case 6:
                    DefaultGUI.Label(cellRect, item.info.RemoveCount.ToString(), args.selected, args.focused);
                    break;
                default:
                    break;
            }

            GUI.color = old;
        }


        #region Sort

        internal enum SortOption
        {
            Type,
            AcquireCount,
            ReleaseCount,
            UsingCount,
            UnusedCount,
            AddCount,
            RemoveCount,
        }

        private SortOption[] m_SortOptions =
        {
            SortOption.Type,
            SortOption.AcquireCount,
            SortOption.ReleaseCount,
            SortOption.UsingCount,
            SortOption.UnusedCount,
            SortOption.AddCount,
            SortOption.RemoveCount
        };

        private void OnSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            SortIfNeeded(rootItem, GetRows());
        }

        private void SortIfNeeded(TreeViewItem root, IList<TreeViewItem> rows)
        {
            if (rows.Count <= 1)
                return;

            if (multiColumnHeader.sortedColumnIndex == -1)
                return;

            SortByColumn();

            rows.Clear();
            for (int i = 0; i < root.children.Count; i++)
                rows.Add(root.children[i]);

            Repaint();
        }

        private void SortByColumn()
        {
            var sortedColumns = multiColumnHeader.state.sortedColumns;

            if (sortedColumns.Length == 0)
                return;

            List<SharedPoolItem> assetList = new List<SharedPoolItem>();
            foreach (var item in rootItem.children)
            {
                assetList.Add(item as SharedPoolItem);
            }
            var orderedItems = InitialOrder(assetList, sortedColumns);

            rootItem.children = orderedItems.Cast<TreeViewItem>().ToList();
        }

        private IOrderedEnumerable<SharedPoolItem> InitialOrder(IEnumerable<SharedPoolItem> myTypes, int[] columnList)
        {
            SortOption sortOption = m_SortOptions[columnList[0]];
            bool ascending = multiColumnHeader.IsSortedAscending(columnList[0]);
            switch (sortOption)
            {
                case SortOption.Type:
                    return myTypes.Order(l => l.displayName, ascending);
                case SortOption.AcquireCount:
                    return myTypes.Order(l => l.info.AcquireCount, ascending);
                case SortOption.ReleaseCount:
                    return myTypes.Order(l => l.info.ReleaseCount, ascending);
                case SortOption.UsingCount:
                    return myTypes.Order(l => l.info.UsingCount, ascending);
                case SortOption.UnusedCount:
                    return myTypes.Order(l => l.info.UnusedCount, ascending);
                case SortOption.AddCount:
                    return myTypes.Order(l => l.info.AddCount, ascending);
                case SortOption.RemoveCount:
                    return myTypes.Order(l => l.info.RemoveCount, ascending);
                default:
                    return myTypes.Order(l => l.displayName, ascending);
            }
        }

        #endregion


        #endregion
    }

    internal static class MyExtensionMethods
    {
        internal static IOrderedEnumerable<T> Order<T, TKey>(this IEnumerable<T> source, System.Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
            {
                return source.OrderBy(selector);
            }
            else
            {
                return source.OrderByDescending(selector);
            }
        }
    }
}
