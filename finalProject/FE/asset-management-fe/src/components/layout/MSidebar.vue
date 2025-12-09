<template>
  <aside :class="['m-sidebar', { 'm-sidebar--collapsed': isCollapsed }]">
    <div class="m-sidebar__header">
      <div class="m-sidebar__logo">
        <svg width="32" height="32" viewBox="0 0 36 36" fill="none" xmlns="http://www.w3.org/2000/svg">
          <use href="#icon-logo" />
        </svg>
      </div>
      <span v-if="!isCollapsed" class="m-sidebar__title">MISA QLTS</span>
    </div>
    <nav class="m-sidebar__nav">
      <div v-for="(item, index) in menuItems" :key="index" class="m-sidebar__menu-group">
        <a :class="[
          'm-sidebar__item',
          { 'm-sidebar__item--active': item.active },
          { 'm-sidebar__item--has-submenu': item.submenu && item.submenu.length > 0 }
        ]" @click="handleItemClick(item)">
          <div class="m-sidebar__icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <use :href="`#${item.active && item.iconActiveId ? item.iconActiveId : item.iconId}`" />
            </svg>
          </div>
          <span v-if="!isCollapsed" class="m-sidebar__label">{{ item.label }}</span>
          <div v-if="!isCollapsed && (item.submenu && item.submenu.length > 0 || item.showCaret)" class="m-sidebar__caret">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <use :href="item.expanded ? '#icon-top-2' : '#icon-bottom-2'" />
            </svg>
          </div>
        </a>
        <div v-if="!isCollapsed && item.expanded && item.submenu && item.submenu.length > 0" class="m-sidebar__submenu">
          <a v-for="(subItem, subIndex) in item.submenu" :key="subIndex" 
             :class="['m-sidebar__submenu-item', { 'm-sidebar__submenu-item--active': subItem.active }]"
             @click.stop="handleSubmenuClick(item, subItem)">
            <div class="m-sidebar__submenu-icon">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor" xmlns="http://www.w3.org/2000/svg">
                <use href="#icon-right" fill="currentColor" />
              </svg>
            </div>
            <span class="m-sidebar__submenu-label">{{ subItem.label }}</span>
          </a>
        </div>
      </div>
    </nav>
    <div class="m-sidebar__footer">
      <UnderDevelopmentDialog 
        v-model="showUnderDevelopmentDialog"
        @close="showUnderDevelopmentDialog = false"
      />
      <button class="m-sidebar__toggle" @click="toggleCollapse" :title="isCollapsed ? 'Mở rộng' : 'Thu gọn'">
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
          <use :href="isCollapsed ? '#icon-right-s' : '#icon-left-s-2'" />
        </svg>
      </button>
    </div>
  </aside>
</template>

<script setup>
import { ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import UnderDevelopmentDialog from '../dialogs/UnderDevelopmentDialog.vue'

const router = useRouter()

// State for under development dialog
const showUnderDevelopmentDialog = ref(false)

// List of menu items that are under development
const underDevelopmentMenus = [
  'Đánh giá lại',
  'Điều chuyển tài sản',
  'Ghi giảm',
  'Tính hao mòn'
]

const props = defineProps({
  collapsed: {
    type: Boolean,
    default: true
  }
})

const emit = defineEmits(['item-click', 'update:collapsed'])

const isCollapsed = ref(props.collapsed)

watch(
  () => props.collapsed,
  (newVal) => {
    isCollapsed.value = newVal
    // Khi menu thu gọn, đóng tất cả submenu
    if (newVal) {
      menuItems.value.forEach((item) => {
        if (item.submenu && item.submenu.length > 0) {
          item.expanded = false
        }
      })
    }
  }
)

const toggleCollapse = () => {
  isCollapsed.value = !isCollapsed.value
  emit('update:collapsed', isCollapsed.value)
}

const menuItems = ref([
  {
    label: 'Tổng quan',
    iconId: 'icon-tong-quan',
    iconActiveId: 'icon-tong-quan-active',
    active: false,
    expanded: false,
    submenu: []
  },
  {
    label: 'Tài sản',
    iconId: 'icon-tai-san',
    iconActiveId: 'icon-tai-san-active',
    active: true,
    expanded: true,
    submenu: [
      {
        label: 'Ghi tăng',
        active: false
      },
      {
        label: 'Đánh giá lại',
        active: false
      },
      {
        label: 'Điều chuyển tài sản',
        active: false
      },
      {
        label: 'Ghi giảm',
        active: false
      },
      {
        label: 'Tính hao mòn',
        active: false
      }
    ]
  },
  {
    label: 'Tài sản HT-ĐB',
    iconId: 'icon-htdb',
    iconActiveId: 'icon-htdb-active',
    active: false,
    expanded: false,
    submenu: [],
    showCaret: true
  },
  {
    label: 'Công cụ dụng cụ',
    iconId: 'icon-ccdc',
    iconActiveId: 'icon-ccdc-active',
    active: false,
    expanded: false,
    submenu: [],
    showCaret: true
  },
  {
    label: 'Danh mục',
    iconId: 'icon-danh-muc',
    iconActiveId: 'icon-danh-muc-active',
    active: false,
    expanded: false,
    submenu: []
  },
  {
    label: 'Tra cứu',
    iconId: 'icon-tra-cuu',
    iconActiveId: 'icon-tra-cuu-active',
    active: false,
    expanded: false,
    submenu: []
  },
  {
    label: 'Báo cáo',
    iconId: 'icon-bao-cao',
    iconActiveId: 'icon-bao-cao-active',
    active: false,
    expanded: false,
    submenu: [],
    showCaret: true
  }
])

const handleItemClick = (item) => {
  // If item has submenu, toggle expand/collapse
  if (item.submenu && item.submenu.length > 0) {
    // Close other expanded menus
    menuItems.value.forEach((i) => {
      if (i !== item && i.submenu && i.submenu.length > 0) {
        i.expanded = false
      }
    })
    item.expanded = !item.expanded
    // Set as active when expanding
    if (item.expanded) {
      menuItems.value.forEach((i) => (i.active = false))
      item.active = true
      // Navigate to default route when expanding "Tài sản"
      if (item.label === 'Tài sản') {
        router.push('/assets')
      }
    }
  } else {
    // If no submenu, set as active
    menuItems.value.forEach((i) => {
      i.active = false
      // Close other expanded menus
      if (i.submenu && i.submenu.length > 0) {
        i.expanded = false
      }
    })
    item.active = true
    emit('item-click', item)
  }
}

const handleSubmenuClick = (parentItem, subItem) => {
  // Check if this menu is under development
  if (underDevelopmentMenus.includes(subItem.label)) {
    showUnderDevelopmentDialog.value = true
    return
  }
  
  // Set parent as active
  menuItems.value.forEach((i) => {
    i.active = false
    if (i.submenu) {
      i.submenu.forEach((sub) => (sub.active = false))
    }
  })
  parentItem.active = true
  subItem.active = true
  
  // Navigate based on submenu item label
  if (subItem.label === 'Ghi tăng') {
    router.push('/increase')
  }
  
  emit('item-click', { ...subItem, parent: parentItem })
}
</script>

<style scoped>
.m-sidebar {
  width: 210px;
  background-color: #1c3048;
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  transition: width 0.3s;
  position: fixed;
  left: 0;
  top: 0;
  z-index: 100;
}

.m-sidebar--collapsed {
  width: 66px;
}

.m-sidebar__header {
  padding: 16px;
  display: flex;
  align-items: center;
  gap: 12px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  position: relative;
}

.m-sidebar__logo {
  color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.m-sidebar--collapsed .m-sidebar__header {
  justify-content: center;
}

.m-sidebar--collapsed .m-sidebar__logo {
  margin: 0 auto;
}

.m-sidebar__title {
  font-size: 16px;
  font-weight: 700;
  color: #ffffff;
  font-family: 'Roboto', sans-serif;
  white-space: nowrap;
}

.m-sidebar__toggle {
  background: none;
  border: none;
  color: #ffffff;
  cursor: pointer;
  padding: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 4px;
  transition: all 0.2s;
  width: 40px;
  height: 40px;
  flex-shrink: 0;
}

.m-sidebar__toggle:hover {
  background-color: rgba(255, 255, 255, 0.1);
}

.m-sidebar__toggle svg {
  width: 24px;
  height: 24px;
  display: block;
}


.m-sidebar__footer {
  padding: 16px;
  display: flex;
  align-items: center;
  justify-content: flex-end;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

.m-sidebar--collapsed .m-sidebar__footer {
  justify-content: center;
  padding: 16px 4px;
}

.m-sidebar__footer .m-sidebar__toggle {
  width: 40px;
  height: 40px;
}

.m-sidebar__toggle-icon--rotated {
  transform: rotate(180deg);
}

.m-sidebar__nav {
  flex: 1;
  padding: 16px 0;
  display: flex;
  flex-direction: column;
  gap: 0;
  overflow-y: auto;
}

.m-sidebar__menu-group {
  display: flex;
  flex-direction: column;
}

.m-sidebar__item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 0 12px 16px;
  color: #ffffff;
  cursor: pointer;
  transition: all 0.2s;
  text-decoration: none;
  margin: 0 8px;
  border-radius: 8px;
  position: relative;
}

.m-sidebar--collapsed .m-sidebar__item {
  justify-content: center;
  padding: 12px;
  margin: 0 auto;
  width: 44px; /** icon khi menu collapsed */
  height: 40px; /** icon khi menu collapsed */
}

.m-sidebar__item:hover {
  background-color: rgba(255, 255, 255, 0.1);
}

.m-sidebar__item--active {
  background-color: #1aa4c8;
  border-radius: 8px;
}

.m-sidebar__item--has-submenu {
  position: relative;
}

.m-sidebar__item--has-submenu .m-sidebar__label {
  flex: 1;
}

.m-sidebar__caret {
  margin-left: auto;
  margin-right: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #ffffff;
  flex-shrink: 0;
  padding: 0;
  width: 24px;
  min-width: 24px;
}

.m-sidebar__caret svg {
  width: 24px;
  height: 24px;
  display: block;
}

.m-sidebar__submenu {
  display: flex;
  flex-direction: column;
  background-color: rgba(216, 211, 211, 0.2);
  margin: 0 8px 0 24px;
  border-radius: 8px;
  padding: 4px 0;
}

.m-sidebar__submenu-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 16px 10px 12px;
  color: #ffffff;
  cursor: pointer;
  transition: all 0.2s;
  text-decoration: none;
  border-radius: 4px;
  margin: 2px 8px;
}

.m-sidebar__submenu-item:hover {
  background-color: rgba(255, 255, 255, 0.1);
}

.m-sidebar__submenu-item--active {
  background-color: rgba(26, 164, 200, 0.3);
}

.m-sidebar__submenu-icon {
  width: 16px;
  height: 16px;
  flex-shrink: 0;
  color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  opacity: 0;
  transition: opacity 0.2s;
}

.m-sidebar__submenu-item:hover .m-sidebar__submenu-icon,
.m-sidebar__submenu-item--active .m-sidebar__submenu-icon {
  opacity: 1;
}

.m-sidebar__submenu-icon svg {
  width: 100%;
  height: 100%;
  fill: currentColor;
  color: #ffffff;
}

.m-sidebar__submenu-icon svg use {
  fill: currentColor;
}

.m-sidebar__submenu-label {
  font-size: 14px;
  font-family: 'Roboto', sans-serif;
  white-space: nowrap;
}

.m-sidebar__icon {
  width: 24px;
  height: 24px;
  flex-shrink: 0;
  color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 24px;
}

.m-sidebar__icon svg {
  width: 24px;
  height: 24px;
  display: block;
  object-fit: contain;
}

.m-sidebar__label {
  font-size: 14px;
  font-family: 'Roboto', sans-serif;
  white-space: nowrap;
  transition: opacity 0.2s;
}

.m-sidebar--collapsed .m-sidebar__label {
  display: none;
}
</style>
