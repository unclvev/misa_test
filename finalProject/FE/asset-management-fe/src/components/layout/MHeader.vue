<template>
  <header class="m-header">
    <div class="m-header__left">
      <span class="m-header__org">Danh sách tài sản</span>
    </div>

    <div class="m-header__right">
      <div class="m-header__actions">
        <span class="m-header__org-name">Sở tài chính</span>
        <div class="m-header__year-wrapper">
          <label class="m-header__year-label">Năm</label>
          <div class="m-header__year-input">
            <a-input-number v-model:value="selectedYear" :min="1970" :max="2099" />
          </div>
        </div>
        <button class="m-header__icon-btn" title="Thông báo">
          <svg width="26" height="26" viewBox="0 0 24 24" fill="currentColor" preserveAspectRatio="xMidYMid meet">
            <use href="#icon-lock" />
          </svg>
        </button>
        <button class="m-header__icon-btn" title="Ứng dụng">
          <svg width="26" height="26" viewBox="0 0 24 24" fill="currentColor" preserveAspectRatio="xMidYMid meet">
            <use href="#icon-he-thong" />
          </svg>
        </button>
        <button class="m-header__icon-btn" title="Trợ giúp">
          <svg width="26" height="26" viewBox="0 0 24 24" fill="currentColor" preserveAspectRatio="xMidYMid meet">
            <use href="#icon-ho-tro" />
          </svg>
        </button>
        <button class="m-header__icon-btn" title="Tài khoản">
          <svg width="26" height="26" viewBox="0 0 24 24" fill="currentColor" preserveAspectRatio="xMidYMid meet">
            <use href="#icon-user" />
          </svg>
        </button>
        <button class="m-header__icon-btn m-header__icon-btn--dropdown" title="Dropdown">
          <svg width="26" height="26" viewBox="0 0 24 24" fill="currentColor" preserveAspectRatio="xMidYMid meet">
            <use href="#icon-dropdown" />
          </svg>
        </button>
      </div>
    </div>
  </header>
</template>

<script setup>
import { ref } from 'vue'
import { InputNumber as AInputNumber } from 'ant-design-vue'

defineEmits(['add-asset'])

const currentYear = new Date().getFullYear()
const selectedYear = ref(currentYear)
</script>

<style scoped>
.m-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24px;
  height: 44px;
  background-color: #ffffff;
  border-bottom: 1px solid #e0e0e0;
  position: sticky;
  top: 0;
  z-index: 10;
}

.m-header__left {
  display: flex;
  align-items: center;
  gap: 16px;
}

.m-header__org {
  font-size: 14px;
  font-weight: 500;
  font-family: 'Roboto', sans-serif;
  color: #001031;
}

.m-header__right {
  display: flex;
  align-items: center;
  gap: 16px;
  flex: 1;
  justify-content: flex-end;
}

.m-header__actions {
  display: flex;
  align-items: center;
  gap: 4px !important;
}

.m-header__org-name {
  padding-right: 26px;
  font-size: 13px;
  color: #666666;
}

.m-header__year-wrapper {
  display: flex;
  align-items: center;
  height: 32px;
  background-color: #e6f7ff;
  overflow: visible;
  gap: 0;
  margin-right: 23px;
}

.m-header__year-label {
  padding: 0 8px 0 12px;
  font-size: 13px;
  font-weight: 400;
  color: #333333;
  font-family: 'Roboto', sans-serif;
  white-space: nowrap;
  height: 100%;
  display: flex;
  align-items: center;
  flex: 0 0 auto;
}

.m-header__year-input {
  height: 100%;
  border: none !important;
  background-color: transparent !important;
  width: 75px;
  flex: 0 0 auto;
  display: flex;
  align-items: center;
}

.m-header__year-input :deep(.ant-input-number) {
  width: 100%;
  height: 100% !important;
  border: none !important;
  background-color: transparent !important;
  box-shadow: none !important;
  display: flex;
  align-items: center;
}

.m-header__year-input :deep(.ant-input-number:hover .ant-input-number-handler-wrap),
.m-header__year-input :deep(.ant-input-number:not(:hover) .ant-input-number-handler-wrap) {
  opacity: 1 !important;
  visibility: visible !important;
}

.m-header__year-input :deep(.ant-input-number-input-wrap) {
  height: 100%;
  display: flex;
  align-items: center;
  flex: 1;
}

.m-header__year-input :deep(.ant-input-number-input) {
  height: 100% !important;
  border: none !important;
  background-color: transparent !important;
  font-size: 13px !important;
  font-weight: 600 !important;
  color: #333333 !important;
  font-family: 'Roboto', sans-serif !important;
  text-align: left !important;
  padding: 0 4px 0 12px !important;
  line-height: 32px !important;
}

.m-header__year-input :deep(.ant-input-number-handler-wrap) {
  display: flex !important;
  flex-direction: column !important;
  border-left: none !important;
  background-color: transparent !important;
  width: auto !important;
  margin-left: 0 !important;
  gap: 0 !important;
  padding: 0 !important;
  opacity: 1 !important;
  visibility: visible !important;
  pointer-events: auto !important;
  z-index: 1 !important;
}

.m-header__year-input :deep(.ant-input-number-handler) {
  background-color: transparent !important;
  border: none !important;
  height: 16px !important;
  width: 20px !important;
  padding: 0 !important;
  margin-left: 0 !important;
  margin-right: 0 !important;
  opacity: 1 !important;
  visibility: visible !important;
  cursor: pointer !important;
  pointer-events: auto !important;
  z-index: 1 !important;
}

.m-header__year-input :deep(.ant-input-number-handler:first-child) {
  border-bottom: none !important;
  margin-bottom: -4px !important;
}

.m-header__year-input :deep(.ant-input-number-handler:last-child) {
  border: none !important;
  margin-top: -8px !important
}

.m-header__year-input :deep(.ant-input-number-handler:hover) {
  background-color: rgba(0, 0, 0, 0.05) !important;
  cursor: pointer !important;
}

.m-header__year-input :deep(.ant-input-number-handler:active) {
  background-color: rgba(0, 0, 0, 0.1) !important;
  cursor: pointer !important;
}

.m-header__year-input :deep(.ant-input-number-handler-up),
.m-header__year-input :deep(.ant-input-number-handler-down) {
  color: #333333 !important;
}

/* Ẩn icon mặc định của Ant Design */
.m-header__year-input :deep(.ant-input-number-handler-up .anticon),
.m-header__year-input :deep(.ant-input-number-handler-down .anticon) {
  display: none !important;
}

/* Đảm bảo các handler có thể click được */
.m-header__year-input :deep(.ant-input-number-handler-up),
.m-header__year-input :deep(.ant-input-number-handler-down) {
  cursor: pointer !important;
  pointer-events: auto !important;
  user-select: none !important;
  -webkit-user-select: none !important;
}

/* Thêm custom icon-up cho nút tăng */
.m-header__year-input :deep(.ant-input-number-handler-up) {
  position: relative;
}

.m-header__year-input :deep(.ant-input-number-handler-up)::before {
  content: '';
  position: absolute;
  top: 40%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: 16px;
  height: 16px;
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'%3E%3Cpath d='M.3,2.882,2.886.292a1,1,0,0,1,1.41,0l2.59,2.59a1,1,0,0,1-.71,1.71H1A1,1,0,0,1,.3,2.882Z' transform='translate(8.41 9.704)' fill='%23333'/%3E%3C/svg%3E");
  background-size: contain;
  background-repeat: no-repeat;
  background-position: center;
  pointer-events: none;
}

/* Thêm custom icon-dropdown cho nút giảm */
.m-header__year-input :deep(.ant-input-number-handler-down) {
  position: relative;
}

.m-header__year-input :deep(.ant-input-number-handler-down)::before {
  content: '';
  position: absolute;
  top: 60%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: 16px;
  height: 16px;
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'%3E%3Cpath d='M.3,1.71,2.886,4.3a1,1,0,0,0,1.41,0l2.59-2.59A1,1,0,0,0,6.176,0H1A1,1,0,0,0,.3,1.71Z' transform='translate(8.41 9.704)' fill='%23333'/%3E%3C/svg%3E");
  background-size: contain;
  background-repeat: no-repeat;
  background-position: center;
  pointer-events: none;
}

.m-header__icon-btn {
  width: 40px;
  height: 40px;
  border: none;
  background-color: transparent;
  color: #666666;
  cursor: pointer;
  border-radius: 4px;
  transition: all 0.2s;
  padding: 0;
  margin: 0;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
}

.m-header__icon-btn svg {
  display: block;
  margin: 0;
  padding: 0;
  flex-shrink: 0;
  line-height: 1;
  width: 24px;
  height: 24px;
}

.m-header__icon-btn:hover {
  background-color: #f5f5f5;
  color: #212121;
}

.m-header__icon-btn--dropdown {
  margin-left: -24px !important;
}

.m-header__icon-btn--dropdown:hover {
  background-color: transparent !important;
  color: #666666 !important;
}
</style>