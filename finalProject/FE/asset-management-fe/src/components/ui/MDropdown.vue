<template>
  <div class="m-dropdown" ref="dropdownRef">
    <div
      :class="[
        'm-dropdown__trigger',
        {
          'm-dropdown__trigger--open': isOpen,
          'm-dropdown__trigger--disabled': disabled,
          'm-dropdown__trigger--error': error
        }
      ]"
      @click="toggleDropdown"
    >
      <svg
        v-if="icon"
        width="20"
        height="20"
        viewBox="0 0 24 24"
        fill="currentColor"
        xmlns="http://www.w3.org/2000/svg"
        class="m-dropdown__prefix-icon"
      >
        <use :href="`#${icon}`" />
      </svg>
      <span class="m-dropdown__value">
        {{ selectedLabel || placeholder }}
      </span>
      <svg
        width="24"
        height="24"
        viewBox="0 0 24 24"
        fill="currentColor"
        xmlns="http://www.w3.org/2000/svg"
        :class="['m-dropdown__icon', { 'm-dropdown__icon--open': isOpen }]"
      >
        <use href="#icon-dropdown" />
      </svg>
    </div>
    <transition name="dropdown">
      <div v-if="isOpen && !disabled" class="m-dropdown__menu">
        <div
          v-if="showAllOption"
          :class="[
            'm-dropdown__item',
            {
              'm-dropdown__item--selected': isAllSelected,
              'm-dropdown__item--hovered': hoveredIndex === 'all'
            }
          ]"
          @click="selectAllOption"
          @mouseenter="hoveredIndex = 'all'"
          @mouseleave="hoveredIndex = null"
        >
          <svg
            width="16"
            height="16"
            viewBox="0 0 24 24"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
            class="m-dropdown__check"
          >
            <use href="#icon-ok-2" />
          </svg>
          <span>{{ allOptionLabel }}</span>
        </div>
        <div
          v-for="(option, index) in options"
          :key="index"
          :class="[
            'm-dropdown__item',
            {
              'm-dropdown__item--selected': isSelected(option),
              'm-dropdown__item--hovered': hoveredIndex === index
            }
          ]"
          @click="selectOption(option)"
          @mouseenter="hoveredIndex = index"
          @mouseleave="hoveredIndex = null"
        >
          <svg
            width="18"
            height="18"
            viewBox="0 0 24 24"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
            class="m-dropdown__check"
          >
            <use href="#icon-ok-2" />
          </svg>
          <span>{{ getOptionLabel(option) }}</span>
        </div>
      </div>
    </transition>
    <span v-if="error" class="m-dropdown__error">{{ error }}</span>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'

const props = defineProps({
  modelValue: {
    type: [String, Number, Object],
    default: null
  },
  options: {
    type: Array,
    required: true
  },
  placeholder: {
    type: String,
    default: 'Dropdown'
  },
  disabled: {
    type: Boolean,
    default: false
  },
  optionLabel: {
    type: String,
    default: 'label'
  },
  optionValue: {
    type: String,
    default: 'value'
  },
  icon: {
    type: String,
    default: null
  },
  showAllOption: {
    type: Boolean,
    default: false
  },
  allOptionLabel: {
    type: String,
    default: 'Tất cả'
  },
  error: {
    type: String,
    default: ''
  }
})

const emit = defineEmits(['update:modelValue', 'change', 'blur'])

const isOpen = ref(false)
const dropdownRef = ref(null)
const hoveredIndex = ref(null)

const isAllSelected = computed(() => {
  return !props.modelValue || props.modelValue === '' || props.modelValue === null
})

const selectAllOption = () => {
  emit('update:modelValue', null)
  emit('change', null)
  isOpen.value = false
  emit('blur')
}

const selectedLabel = computed(() => {
  if (!props.modelValue || props.modelValue === '' || props.modelValue === null) {
    return '' // Always show placeholder when value is empty
  }
  const option = props.options.find((opt) => isSelected(opt))
  return option ? getOptionLabel(option) : ''
})

const getOptionLabel = (option) => {
  if (typeof option === 'string' || typeof option === 'number') {
    return option
  }
  return option[props.optionLabel] || option.label || option
}

const isSelected = (option) => {
  if (typeof option === 'string' || typeof option === 'number') {
    return option === props.modelValue
  }
  const optionVal = option[props.optionValue] || option.value || option
  return optionVal === props.modelValue
}

const toggleDropdown = () => {
  if (!props.disabled) {
    isOpen.value = !isOpen.value
  }
}

const selectOption = (option) => {
  let value
  if (typeof option === 'string' || typeof option === 'number') {
    value = option
  } else {
    value = option[props.optionValue] || option.value || option
  }
  emit('update:modelValue', value)
  emit('change', value)
  isOpen.value = false
  emit('blur')
}

const handleClickOutside = (event) => {
  if (dropdownRef.value && !dropdownRef.value.contains(event.target)) {
    if (isOpen.value) {
      emit('blur')
    }
    isOpen.value = false
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})
</script>

<style scoped>
.m-dropdown {
  position: relative;
  width: 100%;
}

.m-dropdown__trigger {
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: 35px;
  min-height: 35px;
  max-height: 35px;
  padding: 8px 12px;
  border: 1px solid #afafaf;
  border-radius: 3px;
  background-color: #ffffff;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 13px;
  font-family: 'Roboto', sans-serif;
  gap: 8px;
  box-sizing: border-box;
}

.m-dropdown__trigger:hover:not(.m-dropdown__trigger--disabled) {
  border-color: #90caf9;
}

.m-dropdown__trigger:active:not(.m-dropdown__trigger--disabled) {
  border-color: #1aa4c8;
}

.m-dropdown__trigger--open {
  border-color: #1aa4c8;
  box-shadow: 0 0 0 2px rgba(26, 164, 200, 0.2);
}

.m-dropdown__trigger--disabled {
  background-color: #f5f5f5;
  border-color: #e0e0e0;
  cursor: not-allowed;
  opacity: 0.6;
}

.m-dropdown__prefix-icon {
  color: #666666;
  flex-shrink: 0;
  margin-right: 8px;
}

.m-dropdown__value {
  flex: 1;
  color: #001031;
}

.m-dropdown__icon {
  transition: transform 0.2s;
  color: #666666;
  flex-shrink: 0;
}

.m-dropdown__icon :deep(rect) {
  display: none;
}

.m-dropdown__icon :deep(path) {
  fill: currentColor;
  stroke: none;
}

.m-dropdown__icon--open {
  transform: rotate(180deg);
}

.m-dropdown__menu {
  position: absolute;
  top: calc(100% + 4px);
  left: 0;
  right: 0;
  background-color: #ffffff;
  border: 1px solid #afafaf;
  border-radius: 3px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  z-index: 1000;
  max-height: 200px;
  overflow-y: auto;
}

.m-dropdown__item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  cursor: pointer;
  transition: background-color 0.2s;
  font-size: 13px;
  font-family: 'Roboto', sans-serif;
  color: #001031;
}

.m-dropdown__item:hover {
  background-color: rgba(26, 164, 200, 0.2);
}

.m-dropdown__item--selected {
  background-color: rgba(26, 164, 200, 0.2);
  color: #001031;
}

.m-dropdown__check {
  color: #000000;
  flex-shrink: 0;
  opacity: 0;
  transition: opacity 0.2s;
}


.m-dropdown__item--hovered .m-dropdown__check,
.m-dropdown__item--selected .m-dropdown__check {
  opacity: 1;
}

.dropdown-enter-active,
.dropdown-leave-active {
  transition: opacity 0.2s, transform 0.2s;
}

.dropdown-enter-from,
.dropdown-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}

.m-dropdown__trigger--error {
  border-color: #f44336;
}

.m-dropdown__trigger--error:hover:not(.m-dropdown__trigger--disabled) {
  border-color: #f44336;
}

.m-dropdown__trigger--error:focus {
  border-color: #f44336;
  box-shadow: 0 0 0 2px rgba(244, 67, 54, 0.2);
}

.m-dropdown__error {
  display: block;
  margin-top: 4px;
  font-size: 12px;
  color: #f44336;
}
</style>

