// utils/date.ts
import dayjs from "dayjs";
import "dayjs/locale/zh-cn"; // 中文支持
import relativeTime from "dayjs/plugin/relativeTime"; // 相对时间插件

dayjs.extend(relativeTime);
dayjs.locale("zh-cn");

// 常用日期格式化
export const formatDate = (
  date: string | Date | null | undefined,
  format: string = "YYYY-MM-DD HH:mm:ss",
) => {
  if (!date) return "-";
  return dayjs(date).format(format);
};

// 短日期格式
export const formatShortDate = (date: string | Date | null | undefined) => {
  return formatDate(date, "YYYY-MM-DD");
};

// 时间格式
export const formatTime = (date: string | Date | null | undefined) => {
  return formatDate(date, "HH:mm:ss");
};

// 相对时间（如：3分钟前）
export const formatRelativeTime = (date: string | Date | null | undefined) => {
  if (!date) return "-";
  return dayjs(date).fromNow();
};

// 判断是否今天
export const isToday = (date: string | Date | null | undefined) => {
  if (!date) return false;
  return dayjs(date).isSame(dayjs(), "day");
};

export default dayjs;
