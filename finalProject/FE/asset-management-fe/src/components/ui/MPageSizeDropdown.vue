<template>
  <div class="m-page-size-dropdown" ref="dropdownRef">
    <div
      :class="[
        'm-page-size-dropdown__trigger',
        {
          'm-page-size-dropdown__trigger--open': isOpen,
          'm-page-size-dropdown__trigger--disabled': disabled
        }
      ]"
      @click="toggleDropdown"
    >
      <span class="m-page-size-dropdown__value">
        {{ modelValue || placeholder }}
      </span>
      <svg
        width="24"
        height="24"
        viewBox="0 0 24 24"
        fill="currentColor"
        xmlns="http://www.w3.org/2000/svg"
        :class="['m-page-size-dropdown__icon', { 'm-page-size-dropdown__icon--open': isOpen }]"
      >
        <use href="#icon-dropdown" />
      </svg>
    </div>
    <transition name="dropdown">
      <div v-if="isOpen && !disabled" class="m-page-size-dropdown__menu">
        <div
          v-for="(option, index) in options"
          :key="index"
          :class="[
            'm-page-size-dropdown__item',
            {
              'm-page-size-dropdown__item--selected': isSelected(option),
              'm-page-size-dropdown__item--hovered': hoveredIndex === index
            }
          ]"
          @click="selectOption(option)"
          @mouseenter="hoveredIndex = index"
          @mouseleave="hoveredIndex = null"
        >
          <span>{{ option }}</span>
        </div>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'

const props = defineProps({
  modelValue: {
    type: [String, Number],
    default: null
  },
  options: {
    type: Array,
    required: true
  },
  placeholder: {
    type: String,
    default: '10'
  },
  disabled: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['update:modelValue', 'change'])

const isOpen = ref(false)
const dropdownRef = ref(null)
const hoveredIndex = ref(null)

const isSelected = (option) => {
  return option === props.modelValue
}

const toggleDropdown = () => {
  if (!props.disabled) {
    isOpen.value = !isOpen.value
  }
}

const selectOption = (option) => {
  emit('update:modelValue', option)
  emit('change', option)
  isOpen.value = false
}

const handleClickOutside = (event) => {
  if (dropdownRef.value && !dropdownRef.value.contains(event.target)) {
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
.m-page-size-dropdown {
  position: relative;
  width: 100%;
}

.m-page-size-dropdown__trigger {
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: 25px;
  min-height: 25px;
  max-height: 25px;
  padding: 0 2px 0 4px;
  border: 1px solid #d0d0d0;
  border-radius: 4px;
  background-color: #ffffff;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 12px;
  font-family: 'Roboto', sans-serif;
  gap: 2px;
  box-sizing: border-box;
  width: 59px;
  min-width: 59px;
  max-width: 59px;
}

.m-page-size-dropdown__trigger:hover:not(.m-page-size-dropdown__trigger--disabled) {
  border-color: #90caf9;
}

.m-page-size-dropdown__trigger:active:not(.m-page-size-dropdown__trigger--disabled) {
  border-color: #1aa4c8;
}

.m-page-size-dropdown__trigger--open {
  border-color: #1aa4c8;
  box-shadow: 0 0 0 2px rgba(26, 164, 200, 0.2);
}

.m-page-size-dropdown__trigger--disabled {
  background-color: #f5f5f5;
  border-color: #e0e0e0;
  cursor: not-allowed;
  opacity: 0.6;
}

.m-page-size-dropdown__value {
  flex: 1;
  color: #001031;
  text-align: left;
  min-width: 0;
  padding-left: 13px;
}

.m-page-size-dropdown__icon {
  transition: transform 0.2s;
  color: #666666;
  flex-shrink: 0;
  width: 16px;
  height: 16px;
}

.m-page-size-dropdown__icon :deep(rect) {
  display: none;
}

.m-page-size-dropdown__icon :deep(path) {
  fill: currentColor;
  stroke: none;
}

.m-page-size-dropdown__icon--open {
  transform: rotate(180deg);
}

.m-page-size-dropdown__menu {
  position: absolute;
  top: auto;
  bottom: calc(100% + 4px);
  left: 0;
  right: 0;
  background-color: #ffffff;
  border: none;
  border-radius: 3px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  z-index: 10000;
  min-width: 50px;
  overflow-y: auto;
}

.m-page-size-dropdown__item {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0 8px;
  cursor: pointer;
  transition: background-color 0.2s;
  font-size: 12px;
  font-family: 'Roboto', sans-serif;
  color: #001031;
  height: 32px;
  line-height: 32px;
}

.m-page-size-dropdown__item:hover {
  background-color: rgba(26, 164, 200, 0.2);
}

.m-page-size-dropdown__item--selected {
  background-color: rgba(26, 164, 200, 0.2);
  color: #001031;
}

.m-page-size-dropdown__item--hovered {
  background-color: rgba(26, 164, 200, 0.2);
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
</style>

