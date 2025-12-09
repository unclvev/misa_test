<template>
  <IconProvider />
  <MToast />
  <div class="app">
    <MSidebar v-model:collapsed="sidebarCollapsed" @item-click="handleSidebarClick" />
    <div class="app__main" :class="{ 'app__main--collapsed': sidebarCollapsed }">
      <MHeader v-if="showHeader" @add-asset="handleAddAsset" />
      <main class="app__content">
        <RouterView />
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { RouterView, useRoute } from 'vue-router'
import IconProvider from './components/common/IconProvider.vue'
import MSidebar from './components/layout/MSidebar.vue'
import MHeader from './components/layout/MHeader.vue'
import MToast from './components/ui/MToast.vue'

const route = useRoute()
const sidebarCollapsed = ref(false)

// Ẩn MHeader khi ở route increase hoặc edit-increase-voucher
const showHeader = computed(() => {
  return route.name !== 'increase' && route.name !== 'edit-increase-voucher'
})

const handleSidebarClick = (item) => {
  console.log('Sidebar item clicked:', item)
}

const handleAddAsset = () => {
  console.log('Add asset clicked')
}
</script>

<style scoped>
.app {
  display: flex;
  height: 100vh;
  background-color: #f4f7ff;
  overflow: hidden;
}

.app__main {
  flex: 1;
  margin-left: 210px;
  display: flex;
  flex-direction: column;
  transition: margin-left 0.3s;
  height: 100vh;
  overflow: hidden;
}

.app__main--collapsed {
  margin-left: 66px;
}

.app__content {
  flex: 1;
  overflow: hidden;
  min-height: 0;
  display: flex;
  flex-direction: column;
}
</style>
