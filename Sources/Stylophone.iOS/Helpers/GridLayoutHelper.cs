using System;
using CoreGraphics;
using UIKit;

namespace Stylophone.iOS.Helpers
{
    /// <summary>
    /// Helper to reflow grid layouts when the safe area changes (sidebar opening mostly), changing item sizes to fit more albums/folders on screen
    /// </summary>
    public static class GridLayoutHelper
    {
        /// <summary>
        /// Lay out the grid within the safe area
        /// </summary>
        public static void FollowSafeArea(UICollectionView collectionView)
        {
            if (collectionView.CollectionViewLayout is UICollectionViewFlowLayout flowLayout)
                flowLayout.SectionInsetReference = UICollectionViewFlowLayoutSectionInsetReference.SafeArea;
        }

        /// <returns>Width of the area the grid's items are laid out in, given the collectionview's width.</returns>
        public static nfloat GetContentWidth(UICollectionView collectionView, nfloat width)
        {
            var sectionInset = (collectionView.CollectionViewLayout as UICollectionViewFlowLayout)?.SectionInset ?? UIEdgeInsets.Zero;
            var safeArea = collectionView.SafeAreaInsets;

            return width - safeArea.Left - safeArea.Right - sectionInset.Left - sectionInset.Right;
        }

        /// <summary>
        /// Size items so that columns of at least the given size fill the width exactly.
        /// Rows then all use the minimum spacing, and stay aligned with each other (including the last one).
        /// </summary>
        public static CGSize GetItemSize(UICollectionView collectionView, nfloat contentWidth, nfloat minItemSize)
        {
            var spacing = (collectionView.CollectionViewLayout as UICollectionViewFlowLayout)?.MinimumInteritemSpacing ?? 0;
            var columns = Math.Max(1, Math.Floor((contentWidth + spacing) / (minItemSize + spacing)));

            // Round down so rounding errors don't push the last column to the next row
            var itemSize = Math.Floor((contentWidth - (columns - 1) * spacing) / columns);
            itemSize = Math.Max(itemSize, 1);

            return new CGSize(itemSize, itemSize);
        }
    }
}
