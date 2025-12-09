<template>
  <div class="icon-view">
    <div class="icon-view__header">
      <h1>Icon Gallery</h1>
      <p>Tổng số icon: {{ icons.length }}</p>
    </div>
    <div class="icon-view__search">
      <input
        v-model="searchQuery"
        type="text"
        placeholder="Tìm kiếm icon..."
        class="icon-view__search-input"
      />
    </div>
    <div class="icon-view__grid">
      <div
        v-for="icon in filteredIcons"
        :key="icon.id"
        class="icon-view__item"
        @click="copyIconId(icon.id)"
      >
        <div class="icon-view__icon-wrapper">
          <svg width="48" height="48" viewBox="0 0 24 24" fill="currentColor" xmlns="http://www.w3.org/2000/svg">
            <use :href="`#${icon.id}`" />
          </svg>
        </div>
        <div class="icon-view__label">{{ icon.name }}</div>
        <div class="icon-view__id">{{ icon.id }}</div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const searchQuery = ref('')

const icons = [
  // Sidebar Icons
  { id: 'icon-tong-quan', name: 'Tổng quan' },
  { id: 'icon-tong-quan-active', name: 'Tổng quan (Active)' },
  { id: 'icon-tai-san', name: 'Tài sản' },
  { id: 'icon-tai-san-active', name: 'Tài sản (Active)' },
  { id: 'icon-htdb', name: 'HTDB' },
  { id: 'icon-htdb-active', name: 'HTDB (Active)' },
  { id: 'icon-ccdc', name: 'CCDC' },
  { id: 'icon-ccdc-active', name: 'CCDC (Active)' },
  { id: 'icon-danh-muc', name: 'Danh mục' },
  { id: 'icon-danh-muc-active', name: 'Danh mục (Active)' },
  { id: 'icon-tra-cuu', name: 'Tra cứu' },
  { id: 'icon-tra-cuu-active', name: 'Tra cứu (Active)' },
  { id: 'icon-bao-cao', name: 'Báo cáo' },
  { id: 'icon-bao-cao-active', name: 'Báo cáo (Active)' },
  
  // Header Icons
  { id: 'icon-lock', name: 'Phản hồi' },
  { id: 'icon-he-thong', name: 'Hệ thống' },
  { id: 'icon-ho-tro', name: 'Hỗ trợ' },
  { id: 'icon-user', name: 'User' },
  
  // Common Icons
  { id: 'icon-tim-kiem', name: 'Tìm kiếm' },
  { id: 'icon-dropdown', name: 'Dropdown' },
  { id: 'icon-check', name: 'Check' },
  { id: 'icon-filter-new', name: 'Filter (New)' },
  { id: 'icon-plus', name: 'Plus' },
  { id: 'icon-excel', name: 'Excel' },
  { id: 'icon-delete', name: 'Delete' },
  { id: 'icon-delete-black', name: 'Delete (Black)' },
  { id: 'icon-edit', name: 'Edit' },
  { id: 'icon-copy', name: 'Copy' },
  { id: 'icon-close', name: 'Close' },
  { id: 'icon-logo', name: 'Logo' },
  { id: 'icon-checkbox-check', name: 'Checkbox Check' },
  
  // Additional Icons
  { id: 'icon-ok', name: 'OK' },
  { id: 'icon-off', name: 'Off' },
  { id: 'icon-on', name: 'On' },
  { id: 'icon-uncheck-disable', name: 'Uncheck Disable' },
  { id: 'icon-both', name: 'Both' },
  { id: 'icon-uncheck', name: 'Uncheck' },
  { id: 'icon-xoa-chip', name: 'Xóa Chip' },
  { id: 'icon-left-s', name: 'Left S' },
  { id: 'icon-left-s-2', name: 'Left S 2' },
  { id: 'icon-right-s', name: 'Right S' },
  { id: 'icon-sap-xep', name: 'Sắp xếp' },
  { id: 'icon-more-2', name: 'More 2' },
  { id: 'icon-more-1', name: 'More 1' },
  { id: 'icon-up', name: 'Up' },
  { id: 'icon-left', name: 'Left' },
  { id: 'icon-right', name: 'Right' },
  { id: 'icon-left-2', name: 'Left 2' },
  { id: 'icon-left-3', name: 'Left 3' },
  { id: 'icon-right-2', name: 'Right 2' },
  { id: 'icon-right-3', name: 'Right 3' },
  { id: 'icon-top', name: 'Top' },
  { id: 'icon-top-2', name: 'Top 2' },
  { id: 'icon-top-3', name: 'Top 3' },
  { id: 'icon-bottom', name: 'Bottom' },
  { id: 'icon-bottom-2', name: 'Bottom 2' },
  { id: 'icon-bottom-3', name: 'Bottom 3' },
  { id: 'icon-right-s-2', name: 'Right S 2' },
  { id: 'icon-back', name: 'Back' },
  { id: 'icon-cai-dat', name: 'Cài đặt' },
  { id: 'icon-calendar', name: 'Calendar' },
  { id: 'icon-combo', name: 'Combo' },
  { id: 'icon-uncombo', name: 'Uncombo' },
  { id: 'icon-xoa-icon', name: 'Xóa Icon' },
  { id: 'icon-l-icon', name: 'L Icon' },
  { id: 'icon-r-icon', name: 'R Icon' },
  { id: 'icon-show', name: 'Show' },
  { id: 'icon-hide', name: 'Hide' },
  { id: 'icon-cai-dat-fill', name: 'Cài đặt Fill' },
  { id: 'icon-an-hien-cot', name: 'Ẩn/Hiện Cột' },
  { id: 'icon-lock', name: 'Lock' },
  { id: 'icon-ho-tro-2', name: 'Hỗ trợ 2' },
  { id: 'icon-download', name: 'Download' },
  { id: 'icon-copy-file', name: 'Copy File' },
  { id: 'icon-warning', name: 'Warning' },
  { id: 'icon-sap-xep-2', name: 'Sắp xếp 2' },
  { id: 'icon-in-ma-vach', name: 'In Mã Vạch' },
  { id: 'icon-in-the', name: 'In Thẻ' },
  { id: 'icon-dieu-chuyen', name: 'Điều chuyển' },
  { id: 'icon-refresh', name: 'Refresh' },
  { id: 'icon-horrizon', name: 'Horrizon' },
  { id: 'icon-vertical', name: 'Vertical' },
  { id: 'icon-list-view', name: 'List View' },
  { id: 'icon-bar', name: 'Bar' },
  { id: 'icon-bar-2', name: 'Bar 2' },
  { id: 'icon-menu-to', name: 'Menu To' },
  { id: 'icon-menu-nho', name: 'Menu Nhỏ' },
  { id: 'icon-ok-2', name: 'OK 2' },
  { id: 'icon-filter-old', name: 'Filter (Old)' },
  { id: 'icon-cross', name: 'Cross' },
]

const filteredIcons = computed(() => {
  if (!searchQuery.value) {
    return icons
  }
  const query = searchQuery.value.toLowerCase()
  return icons.filter(icon => 
    icon.name.toLowerCase().includes(query) || 
    icon.id.toLowerCase().includes(query)
  )
})

const copyIconId = (iconId) => {
  navigator.clipboard.writeText(iconId)
  alert(`Đã copy: ${iconId}`)
}
</script>

<style scoped>
.icon-view {
  padding: 24px;
  max-width: 1400px;
  margin: 0 auto;
  min-height: 100vh;
  background: #f4f7ff;
}

.icon-view__header {
  margin-bottom: 24px;
}

.icon-view__header h1 {
  font-size: 24px;
  font-weight: 600;
  color: #001031;
  margin: 0 0 8px 0;
}

.icon-view__header p {
  font-size: 14px;
  color: #666;
  margin: 0;
}

.icon-view__search {
  margin-bottom: 24px;
}

.icon-view__search-input {
  width: 100%;
  max-width: 400px;
  padding: 8px 12px;
  border: 1px solid #e0e0e0;
  border-radius: 4px;
  font-size: 14px;
}

.icon-view__search-input:focus {
  outline: none;
  border-color: #1aa4c8;
}

.icon-view__grid {
  display: flex;
  flex-direction: row;
  gap: 16px;
  overflow-x: auto;
  padding-bottom: 8px;
}

.icon-view__item {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 16px;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
  background: #fff;
  min-width: 120px;
  flex-shrink: 0;
}

.icon-view__item:hover {
  border-color: #1aa4c8;
  box-shadow: 0 2px 8px rgba(26, 164, 200, 0.1);
  transform: translateY(-2px);
}

.icon-view__icon-wrapper {
  width: 48px;
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 8px;
  background: #f5f5f5;
  border-radius: 4px;
}

.icon-view__icon-wrapper svg {
  width: 100%;
  height: 100%;
}

.icon-view__label {
  font-size: 12px;
  font-weight: 500;
  color: #001031;
  text-align: center;
  margin-bottom: 4px;
}

.icon-view__id {
  font-size: 10px;
  color: #999;
  text-align: center;
  word-break: break-all;
}
</style>

