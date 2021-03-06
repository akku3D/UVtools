﻿/*
 *                     GNU AFFERO GENERAL PUBLIC LICENSE
 *                       Version 3, 19 November 2007
 *  Copyright (C) 2007 Free Software Foundation, Inc. <https://fsf.org/>
 *  Everyone is permitted to copy and distribute verbatim copies
 *  of this license document, but changing it is not allowed.
 */
using System.Drawing;
using System.Text;
using UVtools.Core.Objects;

namespace UVtools.Core.Operations
{
    public class OperationPattern : Operation
    {
        #region Overrides

        public override string Title => "Pattern";
        public override string Description =>
            "Duplicates the model in a rectangular pattern around the build plate.";
        public override string ConfirmationText =>
            $"pattern the object across {Cols} columns and {Rows} rows?";

        public override string ProgressTitle =>
            $"Patterning the object across {Cols} columns and {Rows} rows";

        public override string ProgressAction => "Patterned layers";

        public override StringTag Validate(params object[] parameters)
        {
            var sb = new StringBuilder();

            if (Cols <= 1 && Rows <= 1)
            {
                sb.AppendLine("Either columns or rows must be greater than 1.");
            }
            
            if (!ValidateBounds())
            {
                sb.AppendLine("Your parameters will put the object outside of the build plate, please adjust the margins.");
            }

            return new StringTag(sb.ToString());
        }

        #endregion

        public Enumerations.Anchor Anchor { get; set; }

        public uint ImageWidth { get; }
        public uint ImageHeight { get; }

        public ushort MarginCol { get; set; }
        public ushort MarginRow { get; set; }

        public ushort MaxMarginCol { get; set; }
        public ushort MaxMarginRow { get; set; }

        public ushort Cols { get; set; } = 1;
        public ushort Rows { get; set; } = 1;

        public ushort MaxCols { get; set; }
        public ushort MaxRows { get; set; }

        public Size GetPatternVolume => new Size(Cols * ROI.Width + (Cols - 1) * MarginCol, Rows * ROI.Height + (Rows - 1) * MarginRow);

        public OperationPattern()
        {
        }

        public OperationPattern(Rectangle srcRoi, Size resolution)
        {
            ImageWidth = (uint) resolution.Width;
            ImageHeight = (uint) resolution.Height;

            SetRoi(srcRoi);
        }

        public void SetRoi(Rectangle srcRoi)
        {
            ROI = srcRoi;

            MaxCols = (ushort)(ImageWidth / srcRoi.Width);
            MaxRows = (ushort)(ImageHeight / srcRoi.Height);

            MaxMarginCol = CalculateMarginCol(MaxCols);
            MaxMarginRow = CalculateMarginRow(MaxRows);
        }


        /*public void CalculateDstRoi()
        {
            _dstRoi.Size = SrcRoi.Size;

            switch (Anchor)
            {
                case Anchor.TopLeft:
                    _dstRoi.Location = new Point(0, 0);
                    break;
                case Anchor.TopCenter:
                    _dstRoi.Location = new Point((int)(ImageWidth / 2 - SrcRoi.Width / 2), 0);
                    break;
                case Anchor.TopRight:
                    _dstRoi.Location = new Point((int)(ImageWidth - SrcRoi.Width), 0);
                    break;
                case Anchor.MiddleLeft:
                    _dstRoi.Location = new Point(0, (int)(ImageHeight / 2 - SrcRoi.Height / 2));
                    break;
                case Anchor.MiddleCenter:
                    _dstRoi.Location = new Point((int)(ImageWidth / 2 - SrcRoi.Width / 2), (int)(ImageHeight / 2 - SrcRoi.Height / 2));
                    break;
                case Anchor.MiddleRight:
                    _dstRoi.Location = new Point((int)(ImageWidth - SrcRoi.Width), (int)(ImageHeight / 2 - SrcRoi.Height / 2));
                    break;
                case Anchor.BottomLeft:
                    _dstRoi.Location = new Point(0, (int)(ImageHeight - SrcRoi.Height));
                    break;
                case Anchor.BottomCenter:
                    _dstRoi.Location = new Point((int)(ImageWidth / 2 - SrcRoi.Width / 2), (int)(ImageHeight - SrcRoi.Height));
                    break;
                case Anchor.BottomRight:
                    _dstRoi.Location = new Point((int)(ImageWidth - SrcRoi.Width), (int)(ImageHeight - SrcRoi.Height));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _dstRoi.X += MarginLeft;
            _dstRoi.X -= MarginRight;
            _dstRoi.Y += MarginTop;
            _dstRoi.Y -= MarginBottom;
        }*/

        /// <summary>
        /// Fills the plate with maximum cols and rows
        /// </summary>
        public void Fill()
        {
            Cols = MaxCols;
            MarginCol = MaxMarginCol;

            Rows = MaxRows;
            MarginRow = MaxMarginRow;
        }

        public ushort CalculateMarginCol(ushort cols)
        {
            if (cols <= 1) return 0;
            return (ushort)((ImageWidth - ROI.Width * cols) / cols);
        }

        public ushort CalculateMarginRow(ushort rows)
        {
            if (rows <= 1) return 0;
            return (ushort)((ImageHeight - ROI.Height * rows) / rows);
        }

        public Rectangle GetRoi(ushort col, ushort row)
        {
            var patternVolume = GetPatternVolume;

            return new Rectangle(new Point(
                (int) (col * ROI.Width + col * MarginCol + (ImageWidth - patternVolume.Width) / 2), 
                (int) (row * ROI.Height + row * MarginRow + (ImageHeight - patternVolume.Height) / 2)), ROI.Size);
        }

        public bool ValidateBounds()
        {
            var volume = GetPatternVolume;
            if (volume.Width > ImageWidth || volume.Height > ImageHeight) return false;
            return true;
        }
    }
}
