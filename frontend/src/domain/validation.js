export function hasBlankFields(...values) {
  return values.some((value) => {
    if (Array.isArray(value)) return value.length === 0;
    return value === null || value === undefined || String(value).trim() === "";
  });
}
