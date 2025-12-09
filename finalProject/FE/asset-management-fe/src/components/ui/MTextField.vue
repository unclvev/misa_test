<template>
  <div class="m-text-field">
    <input
      :class="[
        'm-text-field__input',
        {
          'm-text-field__input--error': error,
          'm-text-field__input--disabled': disabled,
          'm-text-field__input--no-spinner': type === 'number' && !showSpinner,
          'm-text-field__input--number-input': type === 'number' && isFocused,
          'm-text-field__input--spinner-padding-20': type === 'number' && showSpinner && spinnerPaddingLeft === 20
        }
      ]"
      :type="type"
      :placeholder="placeholder"
      :value="displayValue"
      :disabled="disabled"
      :readonly="readonly"
      @input="handleInput"
      @keydown="handleKeyDown"
      @paste="handlePaste"
      @focus="handleFocus"
      @blur="handleBlur"
      @wheel="handleWheel"
      ref="inputRef"
    />
    <span v-if="error" class="m-text-field__error">{{ error }}</span>
  </div>
</template>

<script setup>
import { computed, ref, nextTick } from 'vue'

const props = defineProps({
  modelValue: {
    type: [String, Number],
    default: ''
  },
  type: {
    type: String,
    default: 'text'
  },
  placeholder: {
    type: String,
   
  },
  disabled: {
    type: Boolean,
    default: false
  },
  readonly: {
    type: Boolean,
    default: false
  },
  error: {
    type: String,
    default: ''
  },
  showSpinner: {
    type: Boolean,
    default: true
  },
  spinnerPaddingLeft: {
    type: Number,
    default: 8
  }
})

const emit = defineEmits(['update:modelValue', 'focus', 'blur'])

const isFocused = ref(false)
const inputRef = ref(null)

const displayValue = computed(() => {
  if (props.type === 'number') {
    // Kiểm tra nếu có giá trị (kể cả 0)
    if (props.modelValue !== null && props.modelValue !== undefined && props.modelValue !== '') {
      const num = Number(props.modelValue)
      if (!isNaN(num)) {
        // Luôn hiển thị giá trị thuần (không format) để đảm bảo hiển thị đúng
        return String(props.modelValue)
      }
    }
    // Kiểm tra riêng trường hợp 0
    if (props.modelValue === 0 || props.modelValue === '0') {
      return '0'
    }
    // Không có giá trị hợp lệ, trả về empty string để hiển thị placeholder
    return ''
  }
  
  // Với type khác, trả về giá trị hoặc empty string
  if (props.modelValue !== null && props.modelValue !== undefined && props.modelValue !== '') {
    return props.modelValue
  }
  return ''
})

// Xử lý keydown để chặn các phím không phải số
const handleKeyDown = (event) => {
  if (props.type === 'number' && !props.disabled && !props.readonly) {
    const key = event.key
    
    // Cho phép các phím điều khiển
    const allowedKeys = [
      'Backspace', 'Delete', 'Tab', 'Escape', 'Enter',
      'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown',
      'Home', 'End'
    ]
    
    if (allowedKeys.includes(key)) {
      return // Cho phép các phím điều khiển
    }
    
    // Cho phép Ctrl/Cmd + A, C, V, X (copy, paste, cut, select all)
    if ((event.ctrlKey || event.metaKey) && ['a', 'c', 'v', 'x'].includes(key.toLowerCase())) {
      return
    }
    
    // Cho phép số 0-9
    if (/^[0-9]$/.test(key)) {
      return
    }
    
    // Cho phép dấu trừ (-) chỉ khi ở đầu và chưa có dấu trừ
    if (key === '-' && event.target.selectionStart === 0 && !event.target.value.includes('-')) {
      return
    }
    
    // Cho phép dấu chấm (.) cho số thập phân (nếu cần)
    // Nhưng theo yêu cầu, chỉ cho phép số nguyên, nên không cho phép dấu chấm
    
    // Chặn tất cả các ký tự khác
    event.preventDefault()
  }
}

// Xử lý paste để loại bỏ chữ
const handlePaste = (event) => {
  if (props.type === 'number' && !props.disabled && !props.readonly) {
    event.preventDefault()
    
    // Lấy text từ clipboard
    const pastedText = (event.clipboardData || window.clipboardData).getData('text')
    
    // Chỉ giữ lại số và dấu trừ ở đầu
    let cleanedValue = pastedText.replace(/[^0-9-]/g, '')
    
    // Đảm bảo dấu trừ chỉ ở đầu
    if (cleanedValue.includes('-')) {
      cleanedValue = '-' + cleanedValue.replace(/-/g, '')
    }
    
    // Lấy vị trí cursor hiện tại
    const input = event.target
    const start = input.selectionStart
    const end = input.selectionEnd
    const currentValue = input.value
    
    // Chèn giá trị đã làm sạch vào vị trí cursor
    const newValue = currentValue.substring(0, start) + cleanedValue + currentValue.substring(end)
    
    // Loại bỏ tất cả ký tự không phải số (trừ dấu trừ ở đầu)
    let finalValue = newValue.replace(/[^0-9-]/g, '')
    if (finalValue.includes('-')) {
      finalValue = '-' + finalValue.replace(/-/g, '')
    }
    
    emit('update:modelValue', finalValue)
    
    // Đặt cursor sau vị trí đã paste
    nextTick(() => {
      const newCursorPosition = start + cleanedValue.length
      input.setSelectionRange(newCursorPosition, newCursorPosition)
    })
  }
}

const handleInput = (event) => {
  let value = event.target.value
  const input = event.target
  const cursorPosition = input.selectionStart
  
  if (props.type === 'number') {
    // Loại bỏ tất cả ký tự không phải số (trừ dấu trừ ở đầu)
    const oldValue = value
    value = value.replace(/[^0-9-]/g, '')
    
    // Đảm bảo dấu trừ chỉ ở đầu
    if (value.includes('-')) {
      value = '-' + value.replace(/-/g, '')
    }
    
    // Tính toán vị trí cursor mới sau khi loại bỏ ký tự
    const removedChars = oldValue.length - value.length
    const newCursorPosition = Math.max(0, cursorPosition - removedChars)
    
    emit('update:modelValue', value)
    
    // Giữ cursor ở vị trí đúng sau khi update
    nextTick(() => {
      input.setSelectionRange(newCursorPosition, newCursorPosition)
    })
  } else {
    emit('update:modelValue', value)
  }
}

const handleFocus = (event) => {
  isFocused.value = true
  emit('focus', event)
  
  // Đặt cursor ở cuối (bên phải) khi focus
  nextTick(() => {
    const input = event.target
    const length = input.value.length
    input.setSelectionRange(length, length)
  })
}

const handleBlur = (event) => {
  isFocused.value = false
  emit('blur', event)
}

// Xử lý wheel event để tăng/giảm giá trị khi hover và scroll
const handleWheel = (event) => {
  if (props.type === 'number' && props.showSpinner && !props.disabled && !props.readonly) {
    event.preventDefault()
    const currentValue = Number(props.modelValue) || 0
    const delta = event.deltaY < 0 ? 1 : -1
    const newValue = currentValue + delta
    emit('update:modelValue', newValue >= 0 ? newValue : 0)
  }
}
</script>

<style scoped>
.m-text-field {
  width: 100%;
}

.m-text-field__input {
  width: 100%;
  height: 36px;
  padding: 8px 12px;
  border: 1px solid #afafaf;
  border-radius: 3px;
  font-size: 13px;
  font-family: 'Roboto', sans-serif;
  transition: all 0.2s;
  background-color: #ffffff;
  color: #001031;
  box-sizing: border-box;
}

.m-text-field__input[type="number"],
.m-text-field__input--number-input {
  text-align: right;
  padding-right: 8px;
  padding-left: 8px;
  direction: ltr; /* Giữ direction LTR để nhập từ phải sang trái */
}

.m-text-field__input::placeholder {
  color: #9e9e9e;
}

.m-text-field__input:hover:not(:disabled):not(.m-text-field__input--error) {
  border-color: #90caf9;
}

.m-text-field__input:active:not(:disabled):not(.m-text-field__input--error) {
  border-color: #1aa4c8;
}

.m-text-field__input:focus:not(:disabled) {
  outline: none;
  border-color: #1aa4c8;
  box-shadow: 0 0 0 2px rgba(26, 164, 200, 0.2);
}

.m-text-field__input--error {
  border-color: #f44336;
}

.m-text-field__input--error:focus {
  border-color: #f44336;
}

.m-text-field__input--disabled {
  background-color: #f5f5f5;
  border-color: #e0e0e0;
  border-style: dashed;
  cursor: not-allowed;
  opacity: 0.6;
}

.m-text-field__input[readonly] {
  background-color: #f5f5f5;
  border-color: #afafaf;
  cursor: default;
  color: #001031;
}

.m-text-field__error {
  display: block;
  margin-top: 4px;
  font-size: 12px;
  color: #f44336;
}

/* Tất cả input number có spinner đều có cùng padding-right để spinner buttons thẳng hàng */
.m-text-field__input[type="number"]:not(.m-text-field__input--no-spinner) {
  padding-right: 15.5px; /* Đủ chỗ cho spinner buttons, giữ thẳng hàng */
}

.m-text-field__input[type="number"]:not(.m-text-field__input--no-spinner)::-webkit-inner-spin-button,
.m-text-field__input[type="number"]:not(.m-text-field__input--no-spinner)::-webkit-outer-spin-button {
  opacity: 1;
  visibility: visible;
  height: 20px;
  width: 20px;
  cursor: pointer;
  margin-left: 8px; /* Mặc định 8px */
}

/* Spinner có margin-left lớn hơn cho các trường đặc biệt (tạo khoảng cách với text) */
.m-text-field__input--spinner-padding-20::-webkit-inner-spin-button,
.m-text-field__input--spinner-padding-20::-webkit-outer-spin-button {
  margin-left: 20px; /* Tăng margin-left để tạo khoảng cách với text */
}

/* Hover effect cho spinner buttons */
.m-text-field__input[type="number"]:not(.m-text-field__input--no-spinner):hover::-webkit-inner-spin-button,
.m-text-field__input[type="number"]:not(.m-text-field__input--no-spinner):hover::-webkit-outer-spin-button {
  opacity: 0.8;
}

/* Ẩn spinner cho input có class no-spinner */
.m-text-field__input--no-spinner::-webkit-inner-spin-button,
.m-text-field__input--no-spinner::-webkit-outer-spin-button {
  opacity: 0;
  visibility: hidden;
  -webkit-appearance: none;
  appearance: none;
  -moz-appearance: textfield;
}
</style>

