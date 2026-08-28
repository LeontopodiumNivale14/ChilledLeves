using System.Collections.Generic;
using ChilledLeves.Gui.ImGuiTable;
using static ChilledLeves.Utilities.LogInfo.IceLogging;

namespace ChilledLeves.Utilities.LogInfo;

internal class LogTableInfo
{
    public class LogTable : Table<LogEntry>, IDisposable
    {
        private readonly TimeColumn _time = new();
        private readonly CountColumn _count = new();
        private readonly LevelColumn _level = new();
        private readonly CategoryColumn _category = new();
        private readonly MessageColumn _message = new();

        private int _lastSeenCount = -1;

        public LogTable()
        {
            List<Column<LogEntry>> headers = [_time, _count, _level, _category, _message];

            Id = "LogTable_V2";
            Columns = headers;
            Sortable = true;
            Flags |= ImGuiTableFlags.Resizable | ImGuiTableFlags.SizingStretchProp;
        }

        public override void LoadRows()
        {
            Rows.Clear();
            Rows.AddRange(LogSystem.Logs);
        }

        public void Reload()
        {
            Rows.Clear();
            LoadRows();
            RowsLoaded = true;
            IsFilterDirty = true;
            IsSortDirty = true;
        }
    }

    public sealed class TimeColumn : Column<LogEntry>
    {
        public TimeColumn()
        {
            Label = "Time";
            Width = ImGui.CalcTextSize("00:00:00 - 00:00:00").X;
            Flags = ImGuiTableColumnFlags.WidthFixed;
        }

        public override int Compare(LogEntry lhs, LogEntry rhs)
        {
            var lhsTime = lhs.Count > 1 ? lhs.LastOccurrence : lhs.Timestamp;
            var rhsTime = rhs.Count > 1 ? rhs.LastOccurrence : rhs.Timestamp;
            return lhsTime.CompareTo(rhsTime);
        }

        public override void DrawColumn(LogEntry row)
        {
            if (row.Count > 1)
            {
                ImGui.Text($"{row.Timestamp:HH:mm:ss}");
                ImGui.SameLine();
                ImGui.TextDisabled("-");
                ImGui.SameLine();
                ImGui.Text($"{row.LastOccurrence:HH:mm:ss}");
            }
            else
            {
                ImGui.Text(row.Timestamp.ToString("HH:mm:ss"));
            }
        }
    }

    public sealed class CountColumn : ColumnNumber<LogEntry>
    {
        public CountColumn()
        {
            LabelKey = "Count";
            SetFixedWidth(ImGui.CalcTextSize("x000").X);
            Flags = ImGuiTableColumnFlags.WidthFixed;
        }

        public override int ToValue(LogEntry row) => row.Count;

        public override void DrawColumn(LogEntry row)
        {
            if (row.Count > 1)
                ImGui.TextColored(new Vector4(1, 0.5f, 0, 1), $"x{row.Count}");
            else
                ImGui.TextDisabled("1");
        }
    }

    public sealed class LevelColumn : ColumnFlags<LogLevel, LogEntry>
    {
        private LogLevel _filterValue;

        public LevelColumn()
        {
            Label = "Level";
            SetFixedWidth(ImGui.CalcTextSize("Warning").X);
            AllFlags = Enum.GetValues<LogLevel>().Aggregate((a, b) => a | b);
            _filterValue = AllFlags;
            Flags = ImGuiTableColumnFlags.WidthFixed;
        }

        public override string NameKeySpace => "ImGuiTable.ColumnLogLevel";
        public override LogLevel FilterValue => _filterValue;

        public override void SetValue(LogLevel value, bool enable)
        {
            if (enable)
                _filterValue |= value;
            else
                _filterValue &= ~value;
        }

        public override bool ShouldShow(LogEntry row) => FilterValue.HasFlag(row.Level);

        public override int Compare(LogEntry lhs, LogEntry rhs)
            => ((int)lhs.Level).CompareTo((int)rhs.Level);

        public override void DrawColumn(LogEntry row)
        {
            var color = row.Level switch
            {
                LogLevel.Error => new Vector4(1, 0, 0, 1),
                LogLevel.Warning => new Vector4(1, 1, 0, 1),
                LogLevel.Info => new Vector4(0, 1, 1, 1),
                _ => new Vector4(0.7f, 0.7f, 0.7f, 1)
            };
            ImGui.TextColored(color, row.Level.ToString());
        }
    }

    public sealed class CategoryColumn : ColumnString<LogEntry>
    {
        public CategoryColumn()
        {
            Label = "Category";
            Flags = ImGuiTableColumnFlags.WidthFixed;
        }

        public override string ToName(LogEntry row) => row.Category ?? "";

        public override void DrawColumn(LogEntry row)
        {
            ImGui.Text(row.Category ?? "");
        }
    }

    public sealed class MessageColumn : ColumnString<LogEntry>
    {
        public MessageColumn()
        {
            Label = "Message";
            Flags = ImGuiTableColumnFlags.WidthStretch;
        }

        public override string ToName(LogEntry row) => row.Message;

        public override void DrawColumn(LogEntry row)
        {
            ImGui.Text(row.Message);
        }
    }
}
