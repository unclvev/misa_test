<template>
  <div class="m-date-picker" ref="datePickerRef">
    <a-date-picker
      v-model:open="isOpen"
      :value="dateValue"
      @change="handleDateChange"
      :getPopupContainer="getPopupContainer"
      format="DD/MM/YYYY"
      :placeholder="placeholder"
      :disabled="disabled"
      :status="error ? 'error' : ''"
    >
      <template #suffixIcon>
        <svg width="24" height="24" viewBox="0 0 24 24" fill="currentColor" xmlns="http://www.w3.org/2000/svg">
          <use href="#icon-calendar" />
        </svg>
      </template>
    </a-date-picker>
    <span v-if="error" class="m-date-picker__error">{{ error }}</span>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { DatePicker } from 'ant-design-vue'
import dayjs from 'dayjs'

const ADatePicker = DatePicker

const props = defineProps({
  modelValue: {
    type: String,
    default: ''
  },
  placeholder: {
    type: String,
    default: 'Chọn ngày'
  },
  disabled: {
    type: Boolean,
    default: false
  },
  error: {
    type: String,
    default: ''
  }
})

const emit = defineEmits(['update:modelValue', 'focus', 'blur'])

const isOpen = ref(false)
const datePickerRef = ref(null)

const dateValue = computed({
  get: () => {
    if (!props.modelValue) return null
    // Parse từ YYYY-MM-DD sang dayjs object
    return dayjs(props.modelValue, 'YYYY-MM-DD')
  },
  set: (val) => {
    // Không cần set, sẽ xử lý trong handleDateChange
  }
})

const handleDateChange = (date, dateString) => {
  if (date) {
    // dateString từ Ant Design sẽ là format DD/MM/YYYY, cần convert sang YYYY-MM-DD
    const parts = dateString.split('/')
    if (parts.length === 3) {
      const day = parts[0]
      const month = parts[1]
      const year = parts[2]
      const dateStr = `${year}-${month}-${day}`
      emit('update:modelValue', dateStr)
    } else {
      // Nếu dateString đã là YYYY-MM-DD
      emit('update:modelValue', dateString)
    }
  } else {
    emit('update:modelValue', '')
  }
  isOpen.value = false
}

const getPopupContainer = () => {
  return datePickerRef.value || document.body
}
</script>

<style scoped>
.m-date-picker {
  width: 100%;
}

/* Override Ant Design DatePicker styles */
.m-date-picker :deep(.ant-picker) {
  width: 100%;
  height: 36px;
  padding: 8px 40px 8px 12px;
  border: 1px solid #afafaf;
  border-radius: 3px;
  font-size: 13px;
  font-family: 'Roboto', sans-serif;
  background-color: #ffffff;
  color: #001031;
}

.m-date-picker :deep(.ant-picker:hover:not(.ant-picker-disabled)) {
  border-color: #90caf9;
}

.m-date-picker :deep(.ant-picker-focused:not(.ant-picker-disabled)) {
  border-color: #1aa4c8;
  box-shadow: 0 0 0 2px rgba(26, 164, 200, 0.2);
}

.m-date-picker :deep(.ant-picker-input > input) {
  font-size: 13px;
  font-family: 'Roboto', sans-serif;
  color: #001031;
}

.m-date-picker :deep(.ant-picker-input > input::placeholder) {
  color: #9e9e9e;
}

.m-date-picker :deep(.ant-picker-suffix) {
  color: #666666;
  margin-right: -30px;
}

.m-date-picker :deep(.ant-picker-suffix:hover) {
  color: #1aa4c8;
}

.m-date-picker :deep(.ant-picker-status-error) {
  border-color: #f44336;
}

.m-date-picker :deep(.ant-picker-status-error:hover) {
  border-color: #f44336;
}

.m-date-picker :deep(.ant-picker-status-error.ant-picker-focused) {
  border-color: #f44336;
  box-shadow: 0 0 0 2px rgba(244, 67, 54, 0.2);
}

.m-date-picker :deep(.ant-picker-disabled) {
  background-color: #f5f5f5;
  border-color: #e0e0e0;
  border-style: dashed;
  cursor: not-allowed;
  opacity: 0.6;
}


.m-date-picker__error {
  display: block;
  margin-top: 4px;
  font-size: 12px;
  color: #f44336;
}
</style>

