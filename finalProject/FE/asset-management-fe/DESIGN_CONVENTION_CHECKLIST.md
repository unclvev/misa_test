# Design Convention Checklist

File này tổng hợp các phần đã tuân thủ và chưa tuân thủ design convention theo file `00.1.ElementStyle.png`.

## 📋 Table Component

### ✅ Đã tuân thủ đúng convention

| Element | Design Spec | Code | File | Status |
|---------|-------------|------|------|--------|
| **Border Radius** | 3px | `border-radius: 3px` | AssetListView.vue:664, 724 | ✅ |
| **Header Background** | Light grey (#f5f5f5) | `background-color: #f5f5f5` | AssetListView.vue:791 | ✅ |
| **Header Font** | Roboto-bold-13px | `font-size: 13px`, `font-weight: 700`, `font-family: 'Roboto'` | AssetListView.vue:794-796 | ✅ |
| **Header Height** | 40px | `height: 40px` | AssetListView.vue:803 | ✅ |
| **Body Font** | Roboto-13px-regular | `font-size: 13px`, `font-weight: 400` | AssetListView.vue:816, 823 | ✅ |
| **Body Height** | 40px | `height: 40px` | AssetListView.vue:824 | ✅ |
| **Text Color** | #001031 | `color: #001031` | AssetListView.vue:797, 818 | ✅ |
| **Summary Value Color** | #001031 (đã đổi từ đỏ) | `color: #001031` | AssetListView.vue:1183 | ✅ |

### ⚠️ Cần xác nhận/chưa đúng convention

| Element | Design Spec | Code | File | Status | Ghi chú |
|---------|-------------|------|------|--------|---------|
| **Cell Padding** | 16px | `padding: 12px 16px` | AssetListView.vue:792, 815 | ⚠️ | Cần xác nhận padding chính xác |
| **STT Column Padding** | 16px | `padding-left: 16px; padding-right: 20px` | AssetListView.vue:1168-1169 | ⚠️ | STT column có padding khác do checkbox |
| **Summary Cell Padding** | ? | `padding: 16px 16px 12px 16px` | AssetListView.vue:1153 | ⚠️ | Không rõ trong design spec |
| **Summary Row Height** | ? | `height: 48px` | AssetListView.vue:1163 | ⚠️ | Không rõ trong design spec |

---

## 📋 Popup Component

### ✅ Đã tuân thủ đúng convention

| Element | Design Spec | Code | File | Status |
|---------|-------------|------|------|--------|
| **Button Gap** | 10px | `gap: 10px` | MPopup.vue:172 | ✅ |

### ⚠️ Chưa đúng convention

| Element | Design Spec | Code | File | Status | Ghi chú |
|---------|-------------|------|------|--------|---------|
| **Header Top Padding** | 16px | `padding: 20px 16px 16px 16px` | MPopup.vue:122 | ⚠️ | Top padding hiện tại 20px, cần sửa thành 16px |
| **Body Left Padding** | 16px | `padding: 16px 16px 52px 14px` | MPopup.vue:150 | ⚠️ | Left padding hiện tại 14px, cần sửa thành 16px |
| **Body Bottom Padding** | 16px | `padding: 16px 16px 52px 14px` | MPopup.vue:150 | ⚠️ | Bottom padding hiện tại 52px, cần sửa thành 16px |
| **Footer Right Padding** | 16px | `padding: 16px 36px 16px 0` | MPopup.vue:173 | ⚠️ | Right padding hiện tại 36px, cần sửa thành 16px |

---

## 📋 Form Component (AssetForm)

### ⚠️ Chưa đúng convention

| Element | Design Spec | Code | File | Status | Ghi chú |
|---------|-------------|------|------|--------|---------|
| **Vertical Spacing Between Rows** | 16px | `margin-bottom: 20px` | AssetForm.vue:194 | ⚠️ | Cần sửa thành 16px |
| **Horizontal Spacing Between Columns (2 columns)** | 16px | `padding-right: 8px; padding-left: 8px` | AssetForm.vue:198, 202 | ⚠️ | Cần sửa thành 16px (tổng 16px, mỗi bên 8px) |
| **Horizontal Spacing Between Columns (3 columns)** | 16px | `padding-right: 8px; padding-left: 8px` | AssetForm.vue:211, 216 | ⚠️ | Cần sửa thành 16px (tổng 16px, mỗi bên 8px) |

---

## 📝 Ghi chú

1. **Padding trong Table**: Design spec nói 16px nhưng không rõ là padding tổng hay padding-left/right. Code hiện tại dùng `12px 16px` (vertical horizontal).

2. **STT Column**: Do có checkbox nên padding có thể khác với các cột khác. Cần xác nhận với designer.

3. **Summary Row**: Không có thông tin rõ ràng trong design spec về padding và height của summary row.

4. **Form Spacing**: Design spec nói spacing 16px giữa các columns, nhưng code hiện tại dùng 8px mỗi bên (tổng 16px). Cần xác nhận cách tính.

---

## 🔧 Cần sửa

### Priority 1 (Rõ ràng trong design spec):
- [ ] Popup header top padding: 20px → 16px
- [ ] Popup body left padding: 14px → 16px
- [ ] Popup body bottom padding: 52px → 16px
- [ ] Popup footer right padding: 36px → 16px
- [ ] Form row vertical spacing: 20px → 16px

### Priority 2 (Cần xác nhận):
- [ ] Table cell padding: Xác nhận 16px hay 12px 16px
- [ ] STT column padding: Xác nhận với designer
- [ ] Form column horizontal spacing: Xác nhận cách tính (8px mỗi bên hay 16px tổng)

---

## 📅 Cập nhật

- **Ngày tạo**: 2025-01-XX
- **Ngày cập nhật cuối**: 2025-01-XX
- **Design Reference**: `00.1.ElementStyle.png`

