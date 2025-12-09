<template>
  <div class="m-tooltip" ref="tooltipRef" @mouseenter="showTooltip" @mouseleave="hideTooltip">
    <slot name="trigger">
      <svg
        width="16"
        height="16"
        viewBox="0 0 24 24"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
        class="m-tooltip__icon"
      >
        <use href="#icon-ho-tro" />
      </svg>
    </slot>
    <transition name="tooltip">
      <div v-if="isVisible" class="m-tooltip__content">
        <slot>{{ text }}</slot>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref } from 'vue'

defineProps({
  text: {
    type: String,
    default: "It's tooltip here"
  }
})

const isVisible = ref(false)
const tooltipRef = ref(null)

const showTooltip = () => {
  isVisible.value = true
}

const hideTooltip = () => {
  isVisible.value = false
}
</script>

<style scoped>
.m-tooltip {
  position: relative;
  display: inline-block;
}

.m-tooltip__icon {
  width: 20px;
  height: 20px;
  color: #1aa4c8;
  cursor: help;
  background-color: transparent;
  border-radius: 50%;
}

.m-tooltip__content {
  position: absolute;
  bottom: calc(100% + 8px);
  left: 50%;
  transform: translateX(-50%);
  background-color: #616161;
  color: #ffffff;
  padding: 6px 12px;
  border-radius: 4px;
  font-size: 12px;
  font-family: 'Roboto', sans-serif;
  white-space: nowrap;
  z-index: 1000;
  pointer-events: none;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

.m-tooltip__content::after {
  content: '';
  position: absolute;
  top: 100%;
  left: 50%;
  transform: translateX(-50%);
  border: 4px solid transparent;
  border-top-color: #616161;
}

.tooltip-enter-active,
.tooltip-leave-active {
  transition: opacity 0.2s, transform 0.2s;
}

.tooltip-enter-from,
.tooltip-leave-to {
  opacity: 0;
  transform: translateX(-50%) translateY(4px);
}
</style>

