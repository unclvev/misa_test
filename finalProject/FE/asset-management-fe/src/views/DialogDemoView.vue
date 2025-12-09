<template>
  <div class="dialog-demo-view">
    <div class="dialog-demo-view__header">
      <h1 class="dialog-demo-view__title">Dialog Components Demo</h1>
      <p class="dialog-demo-view__subtitle">Click các button bên dưới để xem các dialog</p>
    </div>
    
    <div class="dialog-demo-view__content">
      <div class="dialog-demo-view__grid">
        <div class="dialog-demo-view__card">
          <h3 class="dialog-demo-view__card-title">1. Confirm Cancel Dialog</h3>
          <p class="dialog-demo-view__card-description">
            Xác nhận hủy bỏ khai báo tài sản
          </p>
          <MButton variant="main" @click="showConfirmCancel = true">
            Mở Dialog
          </MButton>
        </div>

        <div class="dialog-demo-view__card">
          <h3 class="dialog-demo-view__card-title">2. Confirm Save Dialog</h3>
          <p class="dialog-demo-view__card-description">
            Xác nhận lưu khi có thay đổi
          </p>
          <MButton variant="main" @click="showConfirmSave = true">
            Mở Dialog
          </MButton>
        </div>

        <div class="dialog-demo-view__card">
          <h3 class="dialog-demo-view__card-title">3. Confirm Delete Dialog</h3>
          <p class="dialog-demo-view__card-description">
            Xác nhận xóa một tài sản
          </p>
          <MButton variant="main" @click="showConfirmDelete = true">
            Mở Dialog
          </MButton>
        </div>

        <div class="dialog-demo-view__card">
          <h3 class="dialog-demo-view__card-title">4. Confirm Multi Delete Dialog</h3>
          <p class="dialog-demo-view__card-description">
            Xác nhận xóa nhiều tài sản
          </p>
          <MButton variant="main" @click="showConfirmMultiDelete = true">
            Mở Dialog
          </MButton>
        </div>

        <div class="dialog-demo-view__card">
          <h3 class="dialog-demo-view__card-title">5. Warning Cannot Delete Dialog</h3>
          <p class="dialog-demo-view__card-description">
            Cảnh báo không thể xóa vì có chứng từ
          </p>
          <MButton variant="main" @click="showWarningCannotDelete = true">
            Mở Dialog
          </MButton>
        </div>

        <div class="dialog-demo-view__card">
          <h3 class="dialog-demo-view__card-title">6. Warning Multi Cannot Delete Dialog</h3>
          <p class="dialog-demo-view__card-description">
            Cảnh báo một số tài sản không thể xóa
          </p>
          <MButton variant="main" @click="showWarningMultiCannotDelete = true">
            Mở Dialog
          </MButton>
        </div>
      </div>
    </div>

    <!-- Dialogs -->
    <ConfirmCancelDialog
      v-model="showConfirmCancel"
      @confirm="handleConfirmCancel"
      @cancel="handleCancelCancel"
    />
    
    <ConfirmSaveDialog
      v-model="showConfirmSave"
      @save="handleSave"
      @dont-save="handleDontSave"
      @cancel="handleCancelSave"
    />
    
    <ConfirmDeleteDialog
      v-model="showConfirmDelete"
      asset-code="TS00001"
      asset-name="Laptop Dell 3576E Black 01"
      @confirm="handleConfirmDelete"
      @cancel="handleCancelDelete"
    />
    
    <ConfirmMultiDeleteDialog
      v-model="showConfirmMultiDelete"
      :count="4"
      @confirm="handleConfirmMultiDelete"
      @cancel="handleCancelMultiDelete"
    />
    
    <WarningCannotDeleteDialog
      v-model="showWarningCannotDelete"
      @close="showWarningCannotDelete = false"
    />
    
    <WarningMultiCannotDeleteDialog
      v-model="showWarningMultiCannotDelete"
      :count="2"
      @confirm="showWarningMultiCannotDelete = false"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'
import MButton from '../components/ui/MButton.vue'
import {
  ConfirmCancelDialog,
  ConfirmSaveDialog,
  ConfirmDeleteDialog,
  ConfirmMultiDeleteDialog,
  WarningCannotDeleteDialog,
  WarningMultiCannotDeleteDialog
} from '../components/dialogs'

const showConfirmCancel = ref(false)
const showConfirmSave = ref(false)
const showConfirmDelete = ref(false)
const showConfirmMultiDelete = ref(false)
const showWarningCannotDelete = ref(false)
const showWarningMultiCannotDelete = ref(false)

const handleConfirmCancel = () => {
  console.log('Confirmed cancel')
  alert('Đã xác nhận hủy bỏ')
}

const handleCancelCancel = () => {
  console.log('Cancelled cancel dialog')
}

const handleSave = () => {
  console.log('Confirmed save')
  alert('Đã lưu thay đổi')
}

const handleDontSave = () => {
  console.log('Confirmed don\'t save')
  alert('Đã hủy bỏ không lưu')
}

const handleCancelSave = () => {
  console.log('Cancelled save dialog')
}

const handleConfirmDelete = () => {
  console.log('Confirmed delete')
  alert('Đã xác nhận xóa tài sản')
}

const handleCancelDelete = () => {
  console.log('Cancelled delete')
}

const handleConfirmMultiDelete = () => {
  console.log('Confirmed multi delete')
  alert('Đã xác nhận xóa nhiều tài sản')
}

const handleCancelMultiDelete = () => {
  console.log('Cancelled multi delete')
}
</script>

<style scoped>
.dialog-demo-view {
  padding: 24px;
  background-color: #f4f7ff;
  min-height: 100vh;
}

.dialog-demo-view__header {
  margin-bottom: 32px;
}

.dialog-demo-view__title {
  font-size: 28px;
  font-weight: 600;
  font-family: 'Roboto', sans-serif;
  color: #001031;
  margin: 0 0 8px 0;
}

.dialog-demo-view__subtitle {
  font-size: 14px;
  font-weight: 400;
  font-family: 'Roboto', sans-serif;
  color: #666666;
  margin: 0;
}

.dialog-demo-view__content {
  max-width: 1200px;
}

.dialog-demo-view__grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 24px;
}

.dialog-demo-view__card {
  background-color: #ffffff;
  border-radius: 4px;
  padding: 24px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.dialog-demo-view__card-title {
  font-size: 18px;
  font-weight: 600;
  font-family: 'Roboto', sans-serif;
  color: #001031;
  margin: 0;
}

.dialog-demo-view__card-description {
  font-size: 14px;
  font-weight: 400;
  font-family: 'Roboto', sans-serif;
  color: #666666;
  margin: 0;
  flex: 1;
}

.dialog-demo-view__card :deep(.m-button) {
  align-self: flex-start;
}
</style>

