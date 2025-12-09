<template>
  <button
    :class="[
      'm-button',
      `m-button--${variant}`,
      { 'm-button--disabled': disabled }
    ]"
    :disabled="disabled"
    @click="handleClick"
  >
    <slot></slot>
  </button>
</template>

<script setup>
const props = defineProps({
  variant: {
    type: String,
    default: 'main', // main, sub, outline
    validator: (value) => ['main', 'sub', 'outline'].includes(value)
  },
  disabled: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['click'])

const handleClick = (event) => {
  if (!props.disabled) {
    emit('click', event)
  }
}
</script>

<style scoped>
.m-button {
  padding: 8px 16px;
  border-radius: 3px;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  border: 1px solid transparent;
  font-family: 'Roboto', sans-serif;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

/* Main Button */
.m-button--main {
  background-color: #1aa4c8;
  color: #ffffff;
  border-color: #1aa4c8;
}

.m-button--main:hover:not(.m-button--disabled) {
  background-color: #ffffff;
  border-color: #1aa4c8;
  color: #1aa4c8;
}

.m-button--main:active:not(.m-button--disabled) {
  background-color: #116e88;
  border-color: #116e88;
}

.m-button--main:focus:not(.m-button--disabled) {
  outline: 2px solid rgba(26, 164, 200, 0.3);
  outline-offset: 2px;
}

.m-button--main.m-button--disabled {
  background-color: rgba(26, 164, 200, 0.3);
  color: #ffffff;
  cursor: not-allowed;
  opacity: 0.6;
}

/* Sub Button */
.m-button--sub {
  background-color: #e3f2fd;
  color: #1aa4c8;
  border-color: #1aa4c8;
}

.m-button--sub:hover:not(.m-button--disabled) {
  background-color: #bbdefb;
  border-color: #1589a8;
  color: #1589a8;
}

.m-button--sub:active:not(.m-button--disabled) {
  background-color: #1aa4c8;
  border-color: #1aa4c8;
  color: #ffffff;
}

.m-button--sub:focus:not(.m-button--disabled) {
  outline: 2px solid rgba(26, 164, 200, 0.3);
  outline-offset: 2px;
}

.m-button--sub.m-button--disabled {
  background-color: transparent;
  color: #9e9e9e;
  border-color: #e0e0e0;
  cursor: not-allowed;
  opacity: 0.6;
}

/* Outline Button */
.m-button--outline {
  background-color: transparent;
  color: #666666;
  border-color: #e0e0e0;
}

.m-button--outline:hover:not(.m-button--disabled) {
  background-color: rgba(0, 0, 0, 0.05);
  border-color: #bdbdbd;
  color: #424242;
}

.m-button--outline:active:not(.m-button--disabled) {
  background-color: #1aa4c8;
  border-color: #1aa4c8;
  color: #ffffff;
}

.m-button--outline:focus:not(.m-button--disabled) {
  outline: 2px solid rgba(26, 164, 200, 0.3);
  outline-offset: 2px;
  border-color: #1aa4c8;
}

.m-button--outline.m-button--disabled {
  background-color: transparent;
  color: #bdbdbd;
  border-color: #f5f5f5;
  cursor: not-allowed;
  opacity: 0.6;
}
</style>

